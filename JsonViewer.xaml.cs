using System;
using System.Collections.Generic;
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
    /// Interaction logic for JsonViewer.xaml
    /// </summary>
    public partial class JsonViewer : UserControl
    {
        // -------------------- Dependency Properties --------------------

        /// <summary>
        /// JSON data to display. Accepts string or object.
        /// </summary>
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

        /// <summary>
        /// Title for the viewer.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                name: "Title",
                propertyType: typeof(string),
                ownerType: typeof(JsonViewer),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: "",
                    flags: FrameworkPropertyMetadataOptions.AffectsRender)
            );

        /// <summary>
        /// Hide all control buttons.
        /// </summary>
        public static readonly DependencyProperty HideButtonsProperty =
            DependencyProperty.Register(
                name: "HideButtons",
                propertyType: typeof(bool),
                ownerType: typeof(JsonViewer),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: false,
                    flags: FrameworkPropertyMetadataOptions.AffectsRender)
            );

        /// <summary>
        /// Show the save button.
        /// </summary>
        public static readonly DependencyProperty ShowSaveButtonProperty =
            DependencyProperty.Register(
                name: "ShowSaveButton",
                propertyType: typeof(bool),
                ownerType: typeof(JsonViewer),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: false,
                    flags: FrameworkPropertyMetadataOptions.AffectsRender)
            );

        /// <summary>
        /// Maximum data length to display.
        /// </summary>
        public static readonly DependencyProperty MaxDataLengthProperty =
            DependencyProperty.Register(
                name: "MaxDataLength",
                propertyType: typeof(int),
                ownerType: typeof(JsonViewer),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: 1000,
                    flags: FrameworkPropertyMetadataOptions.AffectsRender)
            );

        /// <summary>
        /// Style for buttons in the viewer.
        /// </summary>
        public static readonly DependencyProperty ButtonStyleProperty =
            DependencyProperty.Register(
                "ButtonStyle",
                typeof(Style),
                typeof(JsonViewer),
                new PropertyMetadata(null, OnButtonStyleChanged)
            );

        // -------------------- Properties --------------------

        /// <summary>
        /// Gets or sets the JSON data.
        /// </summary>
        public object JSON
        {
            get => GetValue(JSONProperty);
            set => SetValue(JSONProperty, value);
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Gets or sets whether buttons are hidden.
        /// </summary>
        public bool HideButtons
        {
            get => (bool)GetValue(HideButtonsProperty);
            set => SetValue(HideButtonsProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the save button is shown.
        /// </summary>
        public bool ShowSaveButton
        {
            get => (bool)GetValue(ShowSaveButtonProperty);
            set => SetValue(ShowSaveButtonProperty, value);
        }

        /// <summary>
        /// Gets or sets the maximum data length.
        /// </summary>
        public int MaxDataLength
        {
            get => (int)GetValue(MaxDataLengthProperty);
            set => SetValue(MaxDataLengthProperty, value);
        }

        /// <summary>
        /// Gets or sets the style for buttons.
        /// </summary>
        public Style ButtonStyle
        {
            get => (Style)GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }

        // -------------------- Private Fields --------------------

        private const GeneratorStatus Generated = GeneratorStatus.ContainersGenerated;
        private DispatcherTimer _timer;

        // -------------------- Constructor --------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonViewer"/> class.
        /// </summary>
        public JsonViewer()
        {
            InitializeComponent();
            Loaded += (s, e) => UpdateButtonStyles();
        }

        // -------------------- Dependency Property Callbacks --------------------

        /// <summary>
        /// Handles changes to the JSON property.
        /// </summary>
        private static void JSONProperty_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (JsonViewer)d;
            if (e.NewValue is string str)
                control.Load(str);
            else
                control.Load(e.NewValue);
        }

        /// <summary>
        /// Handles changes to the ButtonStyle property.
        /// </summary>
        private static void OnButtonStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as JsonViewer;
            control?.UpdateButtonStyles();
        }

        // -------------------- UI Helpers --------------------

        /// <summary>
        /// Updates the style of all buttons in the visual tree.
        /// </summary>
        private void UpdateButtonStyles()
        {
            if (ButtonStyle != null)
            {
                foreach (var button in FindVisualChildren<Button>(this))
                {
                    var newStyle = new Style(typeof(Button), ButtonStyle);

                    // Copy setters
                    foreach (var setter in button.Style.Setters)
                        newStyle.Setters.Add(setter);

                    // Copy triggers
                    foreach (var trigger in button.Style.Triggers)
                        newStyle.Triggers.Add(trigger);

                    // Copy resources
                    foreach (var resourceKey in button.Style.Resources.Keys)
                        newStyle.Resources[resourceKey] = button.Style.Resources[resourceKey];

                    button.Style = newStyle;
                }
            }
        }

        /// <summary>
        /// Recursively finds all visual children of a given type.
        /// </summary>
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T tChild)
                        yield return tChild;

                    foreach (var childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

        // -------------------- JSON Loading and Manipulation --------------------

        /// <summary>
        /// Loads JSON from a string.
        /// </summary>
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

        /// <summary>
        /// Loads JSON from an object.
        /// </summary>
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
                ToggleFirstItem(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open the JSON string:\r\n" + ex.Message);
            }
        }

        /// <summary>
        /// Iterates through a JToken and updates matching items in the tree.
        /// </summary>
        private void IterateJToken(JToken token)
        {
            foreach (var tok in token)
            {
                if (tok.HasValues)
                {
                    IterateJToken(tok);
                    continue;
                }

                if (UpdateMatchingItemSource((JToken)JsonTreeView.ItemsSource, tok))
                    return;
            }
        }

        /// <summary>
        /// Updates the value of a matching JToken in the tree.
        /// </summary>
        private bool UpdateMatchingItemSource(JToken inside, JToken find)
        {
            foreach (var ins in inside)
            {
                if (ins.HasValues)
                {
                    if (UpdateMatchingItemSource(ins, find))
                        return true;
                    continue;
                }

                if (((JValue)ins).Path == ((JValue)find).Path)
                {
                    ((JValue)ins).Value = ((JValue)find).Value;
                    return true;
                }
            }
            return false;
        }

        // -------------------- TreeView Expansion/Collapse --------------------

        /// <summary>
        /// Expands all items in the tree.
        /// </summary>
        private void ExpandAll(object sender, RoutedEventArgs e)
        {
            ToggleItems(true);
        }

        /// <summary>
        /// Collapses all items in the tree.
        /// </summary>
        private void CollapseAll(object sender, RoutedEventArgs e)
        {
            ToggleItems(false);
        }

        /// <summary>
        /// Toggles expansion state for all items.
        /// </summary>
        private void ToggleItems(bool isExpanded)
        {
            if (JsonTreeView.Items.IsEmpty)
                return;

            var prevCursor = Cursor;
            Cursor = Cursors.Wait;
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal, delegate
            {
                ToggleItems(JsonTreeView, JsonTreeView.Items, isExpanded);
                if (!isExpanded)
                    ToggleFirstItem(JsonTreeView, JsonTreeView.Items, true);
                _timer.Stop();
                Cursor = prevCursor;
            }, Application.Current.Dispatcher);
            _timer.Start();
        }

        /// <summary>
        /// Expands/collapses only the first item in the tree.
        /// </summary>
        private void ToggleFirstItem(bool isExpanded)
        {
            if (JsonTreeView.Items.IsEmpty)
                return;

            var prevCursor = Cursor;
            Cursor = Cursors.Wait;
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal, delegate
            {
                ToggleFirstItem(JsonTreeView, JsonTreeView.Items, isExpanded);
                _timer.Stop();
                Cursor = prevCursor;
            }, Application.Current.Dispatcher);
            _timer.Start();
        }

        /// <summary>
        /// Recursively toggles expansion state for items.
        /// </summary>
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

            void OnItemGenStatusChanged(object? sender, EventArgs e)
            {
                if (itemGen.Status == Generated)
                {
                    Recurse(items, isExpanded, itemGen);
                    itemGen.StatusChanged -= OnItemGenStatusChanged;
                }
            }
        }

        /// <summary>
        /// Toggles expansion state for the first item.
        /// </summary>
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
            }
            else
            {
                itemGen.StatusChanged += OnItemGenStatusChanged;
            }

            void OnItemGenStatusChanged(object? sender, EventArgs e)
            {
                if (itemGen.Status == Generated)
                {
                    if (items.Count > 0)
                    {
                        var tvi = itemGen.ContainerFromItem(items[0]) as TreeViewItem;
                        if (tvi != null)
                            tvi.IsExpanded = isExpanded;
                    }
                    itemGen.StatusChanged -= OnItemGenStatusChanged;
                }
            }
        }

        /// <summary>
        /// Recursively expands/collapses all TreeViewItems.
        /// </summary>
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

        // -------------------- Event Handlers --------------------

        /// <summary>
        /// Handles double-click on a JValue to copy its text to clipboard.
        /// </summary>
        private void JValue_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2)
                return;

            if (sender is TextBlock tb)
                Clipboard.SetText(tb.Text.Trim('\"'));
        }

        /// <summary>
        /// Handles save button click to save JSON to file.
        /// </summary>
        private void btnSaveJSON_Click(object sender, RoutedEventArgs e)
        {
            string path = GetSaveFilePath(Title, "JSON|*.json", "Save JSON");
            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                if (JSON is string s)
                    System.IO.File.WriteAllText(path, s);
                else
                    System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(JSON, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Shows a SaveFileDialog and returns the selected file path.
        /// </summary>
        private string GetSaveFilePath(string fileName, string filter, string title)
        {
            var saveFileDialog1 = new SaveFileDialog
            {
                Filter = filter,
                Title = title,
                FileName = fileName
            };

            return saveFileDialog1.ShowDialog() == true ? saveFileDialog1.FileName : "";
        }
    }
}