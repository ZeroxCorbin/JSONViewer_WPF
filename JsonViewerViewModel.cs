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
        public ObservableCollection<JToken> Children { get; } = new ObservableCollection<JToken>();

        public string Title { get; private set; }

        public void Load(string json, string title)
        {
            Title = title;

            //JsonTreeView.ItemsSource = null;
            //JsonTreeView.Items.Clear();

            Children.Clear();

            try
            {
                var token = JToken.Parse(json);

                if (token != null)
                {
                    Children.Add(token);
                }

                //JsonTreeView.ItemsSource = children;

                //ToggleFirstItem(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open the JSON string:\r\n" + ex.Message);
            }
        }

        public void Clear() => Children.Clear();
    }
}
