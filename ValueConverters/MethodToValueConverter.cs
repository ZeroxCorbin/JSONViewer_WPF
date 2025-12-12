using System.Globalization;
using System.Windows.Data;

namespace JSONViewer_WPF.ValueConverters;

public sealed class MethodToValueConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter is not string methodName)
            return null;
        System.Reflection.MethodInfo? methodInfo = value.GetType().GetMethod(methodName, new Type[0]);
        if (methodInfo == null)
            return null;
        var returnValue = methodInfo.Invoke(value, new object[0]);
        return returnValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException(GetType().Name + " can only be used for one way conversion.");
}
