using Dora.Xamarin.WebView.Common;
using Dora.Xamarin.WebView.Common.Attributes;
using Dora.Xamarin.WebView.Common.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;

namespace Dora.Xamarin.WebView.ViewModel
{
    [RegisterAsService(typeof(INewtonJsonSerializer))]
    public class NewtonJsonSerializer : INewtonJsonSerializer
    {
        private readonly JsonSerializerSettings _settings;
        public NewtonJsonSerializer(IOptions<JsonSerializerSettings> optionsAccessor)
        {
            _settings = Guard.ArgumentNotNull(optionsAccessor, nameof(optionsAccessor)).Value;
        }

        public object Deserialize(string json, Type type)
        {
            if (type == typeof(string))
            {
                return json;
            }
            return JsonConvert.DeserializeObject(json, type, _settings);
        }
        public string Serialize(object value) => JsonConvert.SerializeObject(value, _settings);
    }
}
