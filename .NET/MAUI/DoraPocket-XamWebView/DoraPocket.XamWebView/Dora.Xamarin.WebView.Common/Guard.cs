using System;
using System.Collections.Generic;
using System.Linq;

namespace Dora.Xamarin.WebView.Common
{
    public class Guard
    {
        public static T ArgumentNotNull<T>(T value, string name) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(name);
            return value;
        }

        public static IEnumerable<T> ArgumentNotEmpty<T>(IEnumerable<T> value, string name)
        {
            ArgumentNotNull(value, name);
            if (!value.Any())
                throw new ArgumentException($"Argument value {name} cannot be empty");
            return value;
        }

        public static string ArgumentNotWhiteSpace(string value,string name)
        {
            ArgumentNotNull(value, name);
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Argument value {name} cannot be whitespace");
            return value;
        }
    }
}
