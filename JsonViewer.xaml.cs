using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using JSONViewer_WPF.JsonHelpers;
using Logging.lib;
using Microsoft.Win32;
using Newtonsoft.Json;
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
              propertyType: typeof(object),
              ownerType: typeof(JsonViewer),
              typeMetadata: new FrameworkPropertyMetadata(
                  defaultValue: null,
                  flags: FrameworkPropertyMetadataOptions.AffectsRender,
                  propertyChangedCallback: new PropertyChangedCallback(JSONProperty_OnChanged))
            );
        private static void JSONProperty_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (JsonViewer)d;
            if (e.NewValue is string str)
                control.Load(str);
            else
                control.Load(e.NewValue);
        }
        public object JSON
        {
            get => GetValue(JSONProperty);
            set => SetValue(JSONProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
            name: "Title",
            propertyType: typeof(string),
            ownerType: typeof(JsonViewer),
            typeMetadata: new FrameworkPropertyMetadata(
                defaultValue: "",
                flags: FrameworkPropertyMetadataOptions.AffectsRender)
            );

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }


        public static readonly DependencyProperty HideButtonsProperty =
            DependencyProperty.Register(
            name: "HideButtons",
            propertyType: typeof(bool),
            ownerType: typeof(JsonViewer),
            typeMetadata: new FrameworkPropertyMetadata(
                defaultValue: false,
                flags: FrameworkPropertyMetadataOptions.AffectsRender)
            );
        public bool HideButtons
        {
            get => (bool)GetValue(HideButtonsProperty);
            set => SetValue(HideButtonsProperty, value);
        }

        public static readonly DependencyProperty ShowSaveButtonProperty =
            DependencyProperty.Register(
            name: "ShowSaveButton",
            propertyType: typeof(bool),
            ownerType: typeof(JsonViewer),
            typeMetadata: new FrameworkPropertyMetadata(
                defaultValue: false,
                flags: FrameworkPropertyMetadataOptions.AffectsRender)
            );
        public bool ShowSaveButton
        {
            get => (bool)GetValue(ShowSaveButtonProperty);
            set => SetValue(ShowSaveButtonProperty, value);
        }

        public static readonly DependencyProperty MaxDataLengthProperty =
            DependencyProperty.Register(
            name: "MaxDataLength",
            propertyType: typeof(int),
            ownerType: typeof(JsonViewer),
            typeMetadata: new FrameworkPropertyMetadata(
                defaultValue: 1000,
                flags: FrameworkPropertyMetadataOptions.AffectsRender)
            );
        public int MaxDataLength
        {
            get => (int)GetValue(MaxDataLengthProperty);
            set => SetValue(MaxDataLengthProperty, value);
        }

        public static readonly DependencyProperty ButtonStyleProperty =
            DependencyProperty.Register("ButtonStyle", typeof(Style), typeof(JsonViewer), new PropertyMetadata(null, OnButtonStyleChanged));
        public Style ButtonStyle
        {
            get => (Style)GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }
        private static void OnButtonStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as JsonViewer;
            control?.UpdateButtonStyles();
        }

        private void UpdateButtonStyles()
        {
            if (ButtonStyle != null)
            {
                //var derivedButtonStyle = new Style(typeof(Button), ButtonStyle);

                foreach (var button in FindVisualChildren<Button>(this))
                {
                    var newStyle = new Style(typeof(Button), ButtonStyle);

                    // Copy setters
                    foreach (var setter in button.Style.Setters)
                    {
                        newStyle.Setters.Add(setter);
                    }

                    // Copy triggers
                    foreach (var trigger in button.Style.Triggers)
                    {
                        newStyle.Triggers.Add(trigger);
                    }

                    // Copy resources
                    foreach (var resourceKey in button.Style.Resources.Keys)
                    {
                        newStyle.Resources[resourceKey] = button.Style.Resources[resourceKey];
                    }

                    button.Style = newStyle;
                }
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (var childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }


        public JsonViewer()
        {
            InitializeComponent();

            this.Loaded +=  (s, e) => UpdateButtonStyles();
            ;
        }

        private void IterateJToken(JToken token)
        {
            foreach (var tok in token)
            {
                //If the Token has children, loop through them first
                if (tok.HasValues)
                {
                    IterateJToken(tok);
                    continue;
                }

                //If the Token is a value and not a parent, then check for a matching Token
                if (UpdateMatchingItemSource((JToken)JsonTreeView.ItemsSource, tok))
                    return;
            }
        }

        private bool UpdateMatchingItemSource(JToken inside, JToken find)
        {
            foreach (var ins in inside)
            {
                //If the Token has children, loop through them first
                if (ins.HasValues)
                {
                    if (UpdateMatchingItemSource(ins, find))
                        return true;
                    continue;
                }

                //var customObject = ins.ToObject<CustomNotifyPropertyChangedObject>();

                if (((JValue)ins).Path == ((JValue)find).Path)
                {
                    ((JValue)ins).Value = ((JValue)find).Value;
                    return true;
                }
                else
                {

                }

            }

            return false;
        }

        private void Load(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                JsonTreeView.ItemsSource = null;
                return;
            }


            try
            {
                JsonTreeView.ItemsSource = JsonConvert.DeserializeObject<JToken>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open the JSON string:\r\n" + ex.Message);
            }
        }

        private void Load(object obj)
        {
            if (obj == null)
            {
                JsonTreeView.ItemsSource = null;
                return;
            }


            try
            {
                JsonTreeView.ItemsSource = JObject.FromObject(obj);
                //JsonTreeView.ItemsSource = JToken.FromObject(obj);
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
                itemGen.StatusChanged += OnItemGenStatusChanged;
            }

            void OnItemGenStatusChanged(object sender, EventArgs e)
            {
                if (itemGen.Status == Generated)
                {
                    Recurse(items, isExpanded, itemGen);
                    itemGen.StatusChanged -= OnItemGenStatusChanged; // Unsubscribe
                }
            }
        }

        private void ToggleFirstItem(ItemsControl parentContainer, ItemCollection items, bool isExpanded)
        {
            var itemGen = parentContainer.ItemContainerGenerator;
            if (itemGen.Status == Generated)
            {
                if (items.Count == 0)
                    return;

                var tvi = itemGen.ContainerFromItem(items[0]) as TreeViewItem;
                if (tvi != null)
                    tvi.IsExpanded = isExpanded;

                //if(tvi.Items.Count == 1)
                //{
                //    ToggleFirstItem(parentContainer, tvi.Items, isExpanded);
                //}
            }
            else
            {
                itemGen.StatusChanged += OnItemGenStatusChanged;
            }

            void OnItemGenStatusChanged(object sender, EventArgs e)
            {
                if (itemGen.Status == Generated)
                {
                    if (items.Count > 0)
                    {
                        var tvi = itemGen.ContainerFromItem(items[0]) as TreeViewItem;
                        if (tvi != null)
                        {
                            tvi.IsExpanded = isExpanded;
                            //if (tvi.Items.Count == 1)
                            //{
                            //    ToggleFirstItem(parentContainer, tvi.Items, isExpanded);
                            //}
                        }
                    }
                    itemGen.StatusChanged -= OnItemGenStatusChanged; // Unsubscribe
                }
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

        private void btnSaveJSON_Click(object sender, RoutedEventArgs e)
        {
            string path;
            if ((path = GetSaveFilePath(Title, "JSON|*.json", "Save JSON")) == "")
                return;

            try
            {
                if(JSON is string s)
                    System.IO.File.WriteAllText(path, s);
                else
                    System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(JSON, Formatting.Indented));
            }
            catch(Exception ex)
            {
                Logger.Error(ex);
            }

        }
        private string GetSaveFilePath(string fileName, string filter, string title)
        {
            var saveFileDialog1 = new SaveFileDialog
            {
                Filter = filter,//|Gif Image|*.gif|JPeg Image|*.jpg";
                Title = title,
                FileName = fileName
            };

            if (saveFileDialog1.ShowDialog() == true)
                return saveFileDialog1.FileName;
            else
                return "";
        }
    }
}
