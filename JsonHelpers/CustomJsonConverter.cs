using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace JSONViewer_WPF.JsonHelpers
{
    public class CustomJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(JToken).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);

            if (token.Type == JTokenType.Object)
            {
                var jsonObject = (JObject)token;
                var customObject = CreateCustomObject(jsonObject, serializer);
                return JToken.FromObject(customObject, serializer);
            }

            return token;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken token = value as JToken ?? JToken.FromObject(value, serializer);
            token.WriteTo(writer);
        }

        private object CreateCustomObject(JObject jsonObject, JsonSerializer serializer)
        {
            var properties = jsonObject.Properties().ToList();
            var customObject = new Dictionary<string, object>();

            foreach (var property in properties)
            {
                if (property.Value.Type == JTokenType.Object)
                {
                    // Recursive call for nested objects
                    customObject[property.Name] = CreateCustomObject((JObject)property.Value, serializer);
                }
                else
                {
                    // Convert non-object properties to CustomNotifyPropertyChangedObject
                    var notifyPropertyChangedObject = new CustomNotifyPropertyChangedObject(property.Value.ToObject<object>(serializer));
                    customObject[property.Name] = notifyPropertyChangedObject;// JToken.FromObject(notifyPropertyChangedObject, serializer);
                }
            }

            return customObject;
        }
    }

}
