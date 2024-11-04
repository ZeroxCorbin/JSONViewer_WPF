using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace JSONViewer_WPF.TemplateSelectors
{
    public sealed class JPropertyDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PrimitivePropertyTemplate { get; set; }
        public DataTemplate ComplexPropertyTemplate { get; set; }
        public DataTemplate ArrayPropertyTemplate { get; set; }
        public DataTemplate ObjectPropertyTemplate { get; set; }

        public DataTemplate HexPropertyTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if(item == null)
                return null;

            var frameworkElement = container as FrameworkElement;
            if(frameworkElement == null)
                return null;

            var type = item.GetType();
            if (type == typeof(JProperty))
            {
                var jProperty = item as JProperty;
                switch (jProperty.Value.Type)
                {
                    case JTokenType.Object:
                        return frameworkElement.FindResource("ObjectPropertyTemplate") as DataTemplate;
                    case JTokenType.Array:
                        return frameworkElement.FindResource("ArrayPropertyTemplate") as DataTemplate;
                    case JTokenType.None:
                        break;
                    case JTokenType.Constructor:
                        break;
                    case JTokenType.Property:
                        break;
                    case JTokenType.Comment:
                        break;
                    case JTokenType.Integer:
                        break;
                    case JTokenType.Float:
                        break;
                    case JTokenType.String:
                        break;
                    case JTokenType.Boolean:
                        break;
                    case JTokenType.Null:
                        break;
                    case JTokenType.Undefined:
                        break;
                    case JTokenType.Date:
                        break;
                    case JTokenType.Raw:
                        break;
                    case JTokenType.Bytes:
                        break;
                    case JTokenType.Guid:
                        break;
                    case JTokenType.Uri:
                        break;
                    case JTokenType.TimeSpan:
                        break;
                    default:
                        return frameworkElement.FindResource("PrimitivePropertyTemplate") as DataTemplate;

                }
            }

            var key = new DataTemplateKey(type);
            return frameworkElement.FindResource(key) as DataTemplate;
        }
    }
}
