using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace JSONViewer_WPF.ValueConverters;

public sealed class JValueConverter : IValueConverter
{
    private int _maxLength = 256;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        _maxLength = parameter is int maxLength ? maxLength : _maxLength;

        if (value is JValue jval && jval.Value != null)
        {
            switch (jval.Type)
            {
                case JTokenType.String:
                    {
                        return jval.Value is string str
                            ? str.Length > _maxLength ? string.Concat("\"", str.AsSpan(0, _maxLength), "...\"") : (object)("\"" + str + "\"")
                            : "\"" + jval.Value + "\"";
                    }

                case JTokenType.Null:
                    return "Null";
                case JTokenType.Bytes:
                    {
                        return jval.Value is byte[] bytes
                            ? bytes.Length > _maxLength ? bytes.Take(_maxLength).ToArray() : (object)bytes
                            : "0x" + BitConverter.ToString((byte[])jval.Value);
                    }

                default:
                    return jval.Value;

            }
        }

        if (value is JProperty jtok)
        {
            if (jtok.Value != null && jtok.Value.Type == JTokenType.Bytes) { }
                return (byte[])jtok.Value.Values<byte>();
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException(GetType().Name + " can only be used for one way conversion.");
    }
}
