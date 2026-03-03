using System.Globalization;
using System.Windows.Data;
using Newtonsoft.Json.Linq;

namespace JSONViewer_WPF.ValueConverters
{
    public sealed class JArrayLengthConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is JArray array)
                return $"[{array.Count}]";

            if (value is JProperty property && property.Value is JArray propertyArray)
                return $"[ {propertyArray.Count} ]";

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(GetType().Name + " can only be used for one way conversion.");
        }
    }
}
