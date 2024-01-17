using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace JSONViewer_WPF.JsonHelpers
{
    public class CustomNotifyPropertyChangedObject : JValue, INotifyPropertyChanged
    {
        public CustomNotifyPropertyChangedObject(object value) : base(value)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
