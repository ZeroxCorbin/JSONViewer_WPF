using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace JSONViewer_WPF.ValueConverters;

public sealed class JValueConverter : IValueConverter
{
    private const int DefaultMaxLength = 256;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var maxLength = parameter is int max ? max : DefaultMaxLength;

        if (value is JValue jval && jval.Value != null)
        {
            switch (jval.Type)
            {
                case JTokenType.String:
                    {
                        return jval.Value is string str
                            ? str.Length > maxLength ? string.Concat("\"", str.AsSpan(0, maxLength), "...\"") : (object)("\"" + str + "\"")
                            : "\"" + jval.Value + "\"";
                    }

                case JTokenType.Null:
                    return "Null";

                case JTokenType.Bytes:
                    {
                        return jval.Value is byte[] bytes
                            ? bytes.Length > maxLength ? bytes.Take(maxLength).ToArray() : bytes
                            : "0x" + BitConverter.ToString((byte[])jval.Value);
                    }

                default:
                    return jval.Value;
            }
        }

        if (value is JProperty jtok && jtok.Value?.Type == JTokenType.Bytes)
            return jtok.Value.Values<byte>().ToArray();

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException(GetType().Name + " can only be used for one way conversion.");
    }
}
