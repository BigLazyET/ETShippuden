using Dora.Xamarin.WebView.Common.Interfaces;

namespace Dora.Xamarin.WebView.Common.Extensions
{
    public static class SerializerExtensions
    {
        public static T Deserialize<T>(this IJsonNetSerializer jsonNetSerializer, string json)
        {
            return (T)jsonNetSerializer.Deserialize(json, typeof(T));
        }

        public static T Deserialize<T>(this INewtonJsonSerializer newtonJsonSerializer, string json)
        {
            return (T)newtonJsonSerializer.Deserialize(json, typeof(T));
        }
    }
}
