using System.Globalization;
using System.Net;

namespace ETLab_MauiPlainPureMode.Conveters
{
    public class IPEndPointToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IPEndPoint endPoint && endPoint is not null)
                return endPoint.ToString();
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
