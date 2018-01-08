using Gang.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Gang.WebSockets.Serialization
{
    public class JsonSerializationService :
        ISerializationService
    {
        readonly JsonSerializerSettings _jsonSerializerSettings;

        public JsonSerializationService()
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        object ISerializationService
            .Deserialize(string value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type, _jsonSerializerSettings);
        }

        string ISerializationService
            .Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, _jsonSerializerSettings);
        }
    }
}
