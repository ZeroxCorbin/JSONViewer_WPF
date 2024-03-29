﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Newtonsoft.Json.Linq;

namespace JSONViewer_WPF.ValueConverters
{
    public sealed class JArrayLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var jToken = value as JToken;
            if (jToken == null)
                return null;//throw new Exception("Wrong type for this converter");

            switch (jToken.Type)
            {
                case JTokenType.Array:
                    var arrayLen = jToken.Children().Count();
                    return string.Format("[{0}]", arrayLen);
                case JTokenType.Property:
                    var propertyArrayLen = jToken.Children().FirstOrDefault().Children().Count();
                    return string.Format("[ {0} ]", propertyArrayLen);
                default:
                    return null;
                    //throw new Exception("Type should be JProperty or JArray");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(GetType().Name + " can only be used for one way conversion.");
        }
    }
}
