using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;

namespace SerialLogger
{
    class SerialPortViewModel:ViewModelBase
    {
        public ObservableCollection<string> frequencyCombo { get; private set; }
        private SerialPort _serialPort;
        public SerialPort serialPort
        {
            get{
                return _serialPort;
            }
            set {
                _serialPort = value;
                NotifyPropertyChanged("serialPort");
            }
        }
        public SerialPort selectedSerialPort
        {
            set {
                if (value != null && _serialPorts.Contains(value))
                {
                    _serialPorts.Remove(value);
                    NotifyPropertyChanged("serialPorts");
                }
            }
        }
        private ObservableCollection<SerialPort> _serialPorts;
        public ObservableCollection<SerialPort> serialPorts{
            get {
                return _serialPorts;
            }
            set {
                _serialPorts = value;
                NotifyPropertyChanged("serialPorts");
            }
        }
        private string _path;
        public string path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                NotifyPropertyChanged("path");
            }
        }
        private string _recordingInProgressLabel;
        public string recordingInProgressLabel
        {
            get
            {
                return _recordingInProgressLabel;
            }
            set
            {
                NotifyPropertyChanged("recordingInProgressLabel");
            }
        }
        private string _recordingButtonText;
        public string recordingButtonText
        {
            get
            {
                return _recordingButtonText;
            }
            set
            {
              
            }
        }
        private string _recordingFrequency;
        public string recordingFrequency
        {
            get
            {
                return _recordingFrequency;
            }
            set
            {
                _recordingFrequency = value.ToString();
                NotifyPropertyChanged("recordingFrequency");
            }
        }
        private ICommand _SubmitCommand;
        public ICommand SubmitCommand
        {
            get
            {
                if (_SubmitCommand == null)
                {
                    _SubmitCommand = new RelayCommand(param => this.Submit(),
                        null);
                }
                return _SubmitCommand;
            }
        }
        private ICommand _RecordCommand;
        public ICommand RecordCommand
        {
            get
            {
                if (_RecordCommand == null)
                {
                    _RecordCommand = new RelayCommand(param => this.Record(),
                        null);
                }
                return _RecordCommand;
            }
        }
        public SerialPortViewModel()
        {
            _serialPort = new SerialPort();
            _serialPorts = new ObservableCollection<SerialPort>();
            serialPorts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(SerialPort_CollectionChanged);
            _recordingButtonText = "Record";
            _recordingInProgressLabel = "";
            frequencyCombo = new ObservableCollection<string> {
                "Hourly",
                "Daily",
                "Weekly"
            };
            recordingPorts = new List<WindowsRecordService>();
        }
        //Whenever new item is added to the collection, am explicitly calling notify property changed
        void SerialPort_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("serialPorts");
        }       
        private void Submit()
        {
            if (serialPort != null)
            {
                serialPorts.Add(serialPort);
                serialPort = new SerialPort();
            }
        }
        bool isRecording = false;

        private List<WindowsRecordService> recordingPorts;
        private void Record()
        {
            isRecording = !isRecording;
            if (!isRecording)
            {
                Stop();
                return;
            }
            foreach(SerialPort s in serialPorts) {
                WindowsRecordService service = new WindowsRecordService(s, _path, Utils.DurationNameToSeconds(_recordingFrequency));
                try
                {
                    service.Record();
                    recordingPorts.Add(service);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            if (recordingPorts.Count > 0)
            {
                _recordingButtonText = "Stop";
                _recordingInProgressLabel = "Recording in progress...";

                NotifyPropertyChanged("recordingButtonText");
                NotifyPropertyChanged("recordingInProgressLabel");
            }
            else
            {
                isRecording = false;
            }
        }
        private void Stop()
        {
            _recordingButtonText = "Record";
            _recordingInProgressLabel = "";

            NotifyPropertyChanged("recordingButtonText");
            NotifyPropertyChanged("recordingInProgressLabel");
            if (recordingPorts != null && recordingPorts.Count > 0)
            {
                while(recordingPorts.Count > 0)
                {
                    recordingPorts[0].Cancel();
                    recordingPorts.RemoveAt(0);
                }
            }
        }
    }
    
}
