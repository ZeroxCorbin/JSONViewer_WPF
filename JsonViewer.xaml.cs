using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;

namespace JSONViewer_WPF
{
    /// <summary>
    /// Interaction logic for JSONViewer_WPF.xaml
    /// </summary>
    public partial class JsonViewer : UserControl
    {
        private const GeneratorStatus Generated = GeneratorStatus.ContainersGenerated;
        private DispatcherTimer _timer;


        //private string json;

        //public string JSON
        //{
        //    get { return json; }
        //    set
        //    { 
        //        json = value;

        //        Load(json, "");
        //    }
        //}

        // Register a dependency property with the specified property name,
        // property type, owner type, and property metadata. Store the dependency
        // property identifier as a public static readonly member of the class.
        public static readonly DependencyProperty JSONProperty =
            DependencyProperty.Register(
              name: "JSON",
              propertyType: typeof(string),
              ownerType: typeof(JsonViewer),
              typeMetadata: new FrameworkPropertyMetadata(
                  defaultValue: "",
                  flags: FrameworkPropertyMetadataOptions.AffectsRender,
                  propertyChangedCallback: new PropertyChangedCallback(JSONProperty_OnChanged))
            );

        private static void JSONProperty_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (JsonViewer)d;
            control.Load((string)e.NewValue, "");
        }

        public string JSON
        {
            get => (string)GetValue(JSONProperty);
            set => SetValue(JSONProperty, value);
        }

        public static readonly DependencyProperty TitleVisibleProperty =
    DependencyProperty.Register(
      name: "TitleVisible",
      propertyType: typeof(bool),
      ownerType: typeof(JsonViewer),
      typeMetadata: new FrameworkPropertyMetadata(
          defaultValue: true,
          flags: FrameworkPropertyMetadataOptions.AffectsRender,
          propertyChangedCallback: new PropertyChangedCallback(TitleVisibleProperty_OnChanged))
    );

        private static void TitleVisibleProperty_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (JsonViewer)d;
            control.Title.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public bool TitleVisible
        {
            get => (bool)GetValue(JSONProperty);
            set => SetValue(JSONProperty, value);
        }

        public JsonViewer()
        {
            InitializeComponent();
        }

        public void Load(string json, string title)
        {
            if (string.IsNullOrEmpty(json))
                return;

            Title.Content = title;

            JsonTreeView.ItemsSource = null;
            JsonTreeView.Items.Clear();

            var children = new List<JToken>();

            try
            {
                var token = JToken.Parse(json);

                if (token != null)
                {
                    children.Add(token);
                }

                JsonTreeView.ItemsSource = children;

                ToggleFirstItem(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open the JSON string:\r\n" + ex.Message);
            }
        }

        private void JValue_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2)
                return;

            var tb = sender as TextBlock;
            if (tb != null)
            {
                Clipboard.SetText(tb.Text);
            }
        }

        private void ExpandAll(object sender, RoutedEventArgs e)
        {
            ToggleItems(true);
        }

        private void CollapseAll(object sender, RoutedEventArgs e)
        {
            ToggleItems(false);
        }

        private void ToggleItems(bool isExpanded)
        {
            if (JsonTreeView.Items.IsEmpty)
                return;

            var prevCursor = Cursor;
            //System.Windows.Controls.DockPanel.Opacity = 0.2;
            //System.Windows.Controls.DockPanel.IsEnabled = false;
            Cursor = Cursors.Wait;
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal, delegate
            {
                ToggleItems(JsonTreeView, JsonTreeView.Items, isExpanded);
                if (!isExpanded)
                    ToggleFirstItem(JsonTreeView, JsonTreeView.Items, true);
                //System.Windows.Controls.DockPanel.Opacity = 1.0;
                //System.Windows.Controls.DockPanel.IsEnabled = true;
                _timer.Stop();
                Cursor = prevCursor;
            }, Application.Current.Dispatcher);
            _timer.Start();
        }

        private void ToggleFirstItem(bool isExpanded)
        {
            if (JsonTreeView.Items.IsEmpty)
                return;

            var prevCursor = Cursor;
            //System.Windows.Controls.DockPanel.Opacity = 0.2;
            //System.Windows.Controls.DockPanel.IsEnabled = false;
            Cursor = Cursors.Wait;
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal, delegate
            {
                ToggleFirstItem(JsonTreeView, JsonTreeView.Items, isExpanded);
                //System.Windows.Controls.DockPanel.Opacity = 1.0;
                //System.Windows.Controls.DockPanel.IsEnabled = true;
                _timer.Stop();
                Cursor = prevCursor;
            }, Application.Current.Dispatcher);
            _timer.Start();
        }

        private void ToggleItems(ItemsControl parentContainer, ItemCollection items, bool isExpanded)
        {
            var itemGen = parentContainer.ItemContainerGenerator;
            if (itemGen.Status == Generated)
            {
                Recurse(items, isExpanded, itemGen);
            }
            else
            {
                itemGen.StatusChanged += delegate
                {
                    Recurse(items, isExpanded, itemGen);
                };
            }
        }

        private void ToggleFirstItem(ItemsControl parentContainer, ItemCollection items, bool isExpanded)
        {
            var itemGen = parentContainer.ItemContainerGenerator;
            if (itemGen.Status == Generated)
            {
                var tvi = itemGen.ContainerFromItem(items[0]) as TreeViewItem;
                tvi.IsExpanded = isExpanded;

                //if(tvi.Items.Count == 1)
                //{
                //    ToggleFirstItem(parentContainer, tvi.Items, isExpanded);
                //}
            }
            else
            {
                itemGen.StatusChanged += delegate
                {
                    var tvi = itemGen.ContainerFromItem(items[0]) as TreeViewItem;
                    tvi.IsExpanded = isExpanded;

                    //if (tvi.Items.Count == 1)
                    //{
                    //    ToggleFirstItem(parentContainer, tvi.Items, isExpanded);
                    //}
                };
            }
        }

        private void Recurse(ItemCollection items, bool isExpanded, ItemContainerGenerator itemGen)
        {
            if (itemGen.Status != Generated)
                return;

            foreach (var item in items)
            {
                var tvi = itemGen.ContainerFromItem(item) as TreeViewItem;
                tvi.IsExpanded = isExpanded;
                ToggleItems(tvi, tvi.Items, isExpanded);
            }
        }
    }
}
