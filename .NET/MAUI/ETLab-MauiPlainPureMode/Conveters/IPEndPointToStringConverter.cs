using System.Globalization;
using System.Net;

namespace ETLab_MauiPlainPureMode.Conveters
{
    public class IPEndPointToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var endpoint = value as IPEndPoint;
            if (endpoint != null)
                return endpoint.ToString();
            return new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
