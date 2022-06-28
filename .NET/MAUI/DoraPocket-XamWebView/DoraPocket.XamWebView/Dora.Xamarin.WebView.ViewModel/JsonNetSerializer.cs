using Dora.Xamarin.WebView.Common.Attributes;
using Dora.Xamarin.WebView.Common.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Dora.Xamarin.WebView.ViewModel
{
    [RegisterAsService(typeof(IJsonNetSerializer))]
    public class JsonNetSerializer : IJsonNetSerializer
    {
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings();

        public JsonNetSerializer()
        {
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Formatting = Formatting.Indented;
        }

        public object Deserialize(string json, Type type)
        {
            if (type == typeof(string))
            {
                return json;
            }
            return JsonConvert.DeserializeObject(json, type, settings);
        }

        public string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, settings);
        }
    }
}
