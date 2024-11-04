using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Controls;

namespace JSONViewer_WPF.TemplateSelectors
{
    public sealed class JPropertyDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PrimitivePropertyTemplate { get; set; }
        public DataTemplate ComplexPropertyTemplate { get; set; }
        public DataTemplate ArrayPropertyTemplate { get; set; }
        public DataTemplate ObjectPropertyTemplate { get; set; }
        public DataTemplate HexPropertyTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
            {
                return null;
            }

            if (container is not FrameworkElement frameworkElement)
            {
                return null;
            }

            System.Type type = item.GetType();
            if (type == typeof(JProperty))
            {
                JProperty? jProperty = item as JProperty;
                return jProperty.Value.Type switch
                {
                    JTokenType.Object => ObjectPropertyTemplate,
                    JTokenType.Array => ArrayPropertyTemplate,
                    JTokenType.Bytes => HexPropertyTemplate,
                    _ => PrimitivePropertyTemplate,
                };
            }

            DataTemplateKey key = new(type);
            return frameworkElement.FindResource(key) as DataTemplate;
        }
    }
}
