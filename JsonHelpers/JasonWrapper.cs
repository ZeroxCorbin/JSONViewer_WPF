using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace JSONViewer_WPF.JsonHelpers
{
    //[JsonConverter(typeof(JsonWrapperConverter))]
    public class JsonWrapper : INotifyPropertyChanged
    {
        private JValue _jsonValue;

        public JValue JsonValue
        {
            get { return _jsonValue; }
            set
            {
                if (_jsonValue != value)
                {
                    _jsonValue = value;
                    OnPropertyChanged(nameof(JsonValue));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
