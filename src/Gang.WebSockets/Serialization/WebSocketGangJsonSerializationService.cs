using Gang.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Gang.WebSockets.Serialization
{
    public class WebSocketGangJsonSerializationService :
        IGangSerializationService
    {
        readonly JsonSerializerSettings _jsonSerializerSettings;

        public WebSocketGangJsonSerializationService()
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        object IGangSerializationService
            .Deserialize(string value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type, _jsonSerializerSettings);
        }

        string IGangSerializationService
            .Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, _jsonSerializerSettings);
        }
    }
}
