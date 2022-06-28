using System;

namespace Dora.Xamarin.WebView.Common.Interfaces
{
    public interface IJsonNetSerializer
    {
        /// <summary>
        /// Deserializes string to specified type of object.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="type">The type of object being deserialized.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        object Deserialize(string json, Type type);

        /// <summary>
        /// Serializes the specified object to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        string Serialize(object value);
    }
}
