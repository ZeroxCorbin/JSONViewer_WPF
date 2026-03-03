using System.Globalization;
using System.Windows.Data;
using Newtonsoft.Json.Linq;

namespace JSONViewer_WPF.ValueConverters;

public sealed class MethodToValueConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter is not string methodName)
            return null;

        // Hot path used by the viewer templates.
        if (methodName == "Children" && value is JToken token)
            return token.Children();

        System.Reflection.MethodInfo? methodInfo = value.GetType().GetMethod(methodName, Type.EmptyTypes);
        return methodInfo?.Invoke(value, Array.Empty<object>());
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException(GetType().Name + " can only be used for one way conversion.");
}
