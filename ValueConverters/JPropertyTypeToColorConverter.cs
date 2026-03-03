using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Newtonsoft.Json.Linq;

namespace JSONViewer_WPF.ValueConverters
{
    public sealed class JPropertyTypeToColorConverter : IValueConverter
    {
        private static readonly Brush StringBrush = CreateBrush(0x4e, 0x9a, 0x06);
        private static readonly Brush NumberBrush = CreateBrush(0xad, 0x7f, 0xa8);
        private static readonly Brush BooleanBrush = CreateBrush(0xc4, 0xa0, 0x00);
        private static readonly Brush NullBrush = Brushes.OrangeRed;
        private static readonly Brush DateBrush = Brushes.LightGreen;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not JProperty jprop)
                return value;

            return jprop.Value.Type switch
            {
                JTokenType.String => StringBrush,
                JTokenType.Float or JTokenType.Integer => NumberBrush,
                JTokenType.Boolean => BooleanBrush,
                JTokenType.Null => NullBrush,
                JTokenType.Date => DateBrush,
                _ => value,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(GetType().Name + " can only be used for one way conversion.");
        }

        private static Brush CreateBrush(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }
    }
}
