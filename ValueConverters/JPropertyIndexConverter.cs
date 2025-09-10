using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Windows.Data;

namespace JSONViewer_WPF.ValueConverters;

public class JObjectIndexConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is JObject obj && values[1] is JArray array)
        {
            var index = array.IndexOf(obj);
            if (obj.First?.HasValues == true)
            {
                if (obj.First.Type == JTokenType.Property)
                {
                    var firstProp = (JProperty)obj.First;
                    if (firstProp.Value.Type is JTokenType.Object or JTokenType.Array)
                    {
                        var valueString1 = firstProp.Name;
                        if (valueString1.Length > 20)
                        {
                            valueString1 = valueString1[..17] + "...";
                        }

                        return $"{{ {index + 1} }} {valueString1}";
                    }

                    var valueString = firstProp.Value.ToString();
                    if (valueString.Length > 20)
                    {
                        valueString = valueString[..17] + "...";
                    }
                    return $"{{ {index + 1} }} {valueString}";
                }
            }
            else
            {
                return $"{{ {index + 1} }} ";
            }
        }
        return "{ }";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}