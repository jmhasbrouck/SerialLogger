using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;


namespace SerialLogger
{
    class WindowsRecordService : IRecordService, IDisposable
    {
        private SerialLogger.SerialPort portInfo;
        private string path;
        private FileStream fs;
        private int duration;
        private DateTime starttime;
        private System.IO.Ports.SerialPort port;
        private System.Timers.Timer newFileTimer;
        private System.Timers.Timer writeTimer;
        private Queue<byte[]> bufferedBytes;
        private Mutex fileMutex;
        private bool isCanceled = false;

        public WindowsRecordService(SerialLogger.SerialPort s, string path, int duration)
        {
            portInfo = s;
            this.duration = duration;
            this.path = path;
        }
        public void Record()
        {
            path = path + "\\COM" + portInfo.portNumber + DateTime.Now.ToString("yyyy-MM-ddTHHmmss");
            try
            {
                Directory.CreateDirectory(path);
                
            }
            catch (Exception e)
            {
                throw new Exception("Problem with the Path! port number: " + portInfo.portNumber + " path name: " + path + "\nUse a \\ for paths");
            }
            try
            {
                port = new System.IO.Ports.SerialPort("COM" + portInfo.portNumber, portInfo.baudRate);
                port.Open();
            }
            catch (IOException e)
            {
                throw new Exception("Problem with serial port: COM" + portInfo.portNumber);
            }
            string filepath = path + "\\" + DateTime.Now.ToString("yyyy-MM-ddTHHmmss") + ".log";
            fs = File.OpenWrite(filepath);
            starttime = DateTime.UtcNow;
            port.DataReceived += port_DataReceived;
            newFileTimer = new System.Timers.Timer();
            newFileTimer.Interval = this.duration /*seconds*/ * 1000 /*milliseconds per second*/;
            newFileTimer.AutoReset = true;
            newFileTimer.Enabled = true;
            newFileTimer.Elapsed += newFileTimer_Elapsed;
            writeTimer = new System.Timers.Timer();
            writeTimer.Interval = 5/*seconds*/ * 1000/*milliseconds per second*/;
            writeTimer.AutoReset = true;
            writeTimer.Enabled = true;
            writeTimer.Elapsed += writeTimer_Elapsed;
            bufferedBytes = new Queue<byte[]>();
            fileMutex = new Mutex();
            isCanceled = false;
        }

        void newFileTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            fs.Flush();
            string filepath = path + "\\" + DateTime.Now.ToString("yyyy-MM-ddTHHmmss") + ".log";
            fileMutex.WaitOne();
            fs.Close();
            fs = null;
            fs = File.OpenWrite(filepath);
            fileMutex.ReleaseMutex();
        }
        void writeTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            while (bufferedBytes.Count > 0)
            {
                byte[] bytes = bufferedBytes.Dequeue();
                fileMutex.WaitOne();
                fs.Write(bytes, 0, bytes.Count());
                fileMutex.ReleaseMutex();
            }
        }
        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string s = port.ReadExisting();
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            bufferedBytes.Enqueue(bytes);
        }
        public void Cancel()
        {
            if (isCanceled)
            {
                return;
            }
            isCanceled = !isCanceled;
            if (port.IsOpen)
            {
                port.DataReceived -= port_DataReceived;
                port.Close();
                port.Dispose();
            }
            if (newFileTimer.Enabled)
            {
                newFileTimer.Enabled = false;
                newFileTimer.Elapsed -= newFileTimer_Elapsed;
                newFileTimer.Dispose();
            }
            if (writeTimer.Enabled)
            {
                writeTimer.Enabled = false;
                writeTimer.Elapsed -= writeTimer_Elapsed;
                writeTimer.Dispose();
            }
            writeTimer_Elapsed(null, null);
            fs.Flush();
            fs.Close();
            fs.Dispose();
            fileMutex.Dispose();
        }

        public void Dispose()
        {
            Cancel();
        }
    }
}
