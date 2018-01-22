using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialLogger
{
    class SerialPort
    {
        public int portNumber
        {
            get;
            set;
        }
        public int baudRate
        {
            get;
            set;
        }
        public SerialPort()
        {
            baudRate = 115200;
            portNumber = 1;
        }
    }
}
