using System.Globalization;
using System.Windows.Data;
using Newtonsoft.Json.Linq;

namespace JSONViewer_WPF.ValueConverters;

// This converter is only used by JProperty tokens whose Value is Array/Object.
public sealed class ComplexPropertyMethodToValueConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string methodName)
            return null;

        // Hot path used by the viewer templates.
        if (methodName == "Children")
        {
            if (value is JProperty property)
                return property.Value.Children();

            if (value is JToken token)
                return token.Children();
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException(GetType().Name + " can only be used for one way conversion.");
    }
}
