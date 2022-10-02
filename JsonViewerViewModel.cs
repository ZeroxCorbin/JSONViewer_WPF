using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace JSONViewer_WPF
{
    public class JsonViewerViewModel : Core.BaseViewModel
    {
        private JToken children;
        public JToken Children { get => children; set => SetProperty(ref children, value); }


        public string Title { get; private set; }

        public void Load(string json, string title)
        {
            Title = title;

            try
            {
                Children = JToken.Parse(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open the JSON string:\r\n" + ex.Message);
            }
        }

        public void Clear() => Children = null;
    }
}
