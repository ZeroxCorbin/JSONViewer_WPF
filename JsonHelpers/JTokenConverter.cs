using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace JSONViewer_WPF.JsonHelpers
{
    public class JTokenConverter : JsonConverter<JToken>
    {
        public override JToken ReadJson(JsonReader reader, Type objectType, JToken existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            if (token.Type == JTokenType.Object || token.Type == JTokenType.Array)
            {
                return token;
            }

            // Wrap primitive values in JsonWrapper
            var wrapper = new JsonWrapper { JsonValue = new JValue(token.ToObject<object>()) };
            return JToken.FromObject(wrapper, serializer);
        }

        public override void WriteJson(JsonWriter writer, JToken value, JsonSerializer serializer)
        {
            // Use default serialization for objects and arrays
            if (value.Type == JTokenType.Object || value.Type == JTokenType.Array)
            {
                value.WriteTo(writer, serializer.Converters.ToArray());
            }
            else
            {
                // Unwrap JsonWrapper for primitive values
                var wrapper = value.ToObject<JsonWrapper>();
                JToken.FromObject(wrapper.JsonValue.Value).WriteTo(writer, serializer.Converters.ToArray());
            }
        }
    }
}
