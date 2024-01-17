using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace JSONViewer_WPF.JsonHelpers
{
    public class JsonWrapperContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            if (objectType == typeof(JValue))
            {
                return base.CreateObjectContract(typeof(JsonWrapper));
            }

            return base.CreateContract(objectType);
        }

    }
}
