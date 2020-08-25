using Gang.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Text;

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

        public TObject Map<TObject>(object json)
        {
            return ((JToken)json).ToObject<TObject>();
        }

        object IGangSerializationService
            .Deserialize(byte[] data, Type type)
        {
            var value = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject(value, type, _jsonSerializerSettings);
        }

        byte[] IGangSerializationService
            .Serialize(object value)
        {
            return
                JsonConvert.SerializeObject(value, _jsonSerializerSettings)
                .GangToBytes();
        }
    }
}
