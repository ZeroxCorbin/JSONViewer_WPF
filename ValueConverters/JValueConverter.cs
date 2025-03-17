using System;
using System.Globalization;
using System.Numerics;
using System.Windows.Data;
using Newtonsoft.Json.Linq;

namespace JSONViewer_WPF.ValueConverters
{
    public sealed class JValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is JValue jval && jval.Value != null)
            {
                switch (jval.Type)
                {
                    case JTokenType.String:
                        return "\"" + jval.Value + "\"";
                    case JTokenType.Null:
                        return "Null";
                    case JTokenType.Bytes:
                        return (byte[])jval.Value;
                    default:
                        return jval.Value;

                }
            }

            if(value is JProperty jtok)
            {
                if (jtok.Value != null && jtok.Value.Type == JTokenType.Bytes)
                    return (byte[])jtok.Value.Values<byte>();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(GetType().Name + " can only be used for one way conversion.");
        }
    }
}
