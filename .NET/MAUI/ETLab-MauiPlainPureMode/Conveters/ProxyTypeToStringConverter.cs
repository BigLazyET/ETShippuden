using STUN.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLab_MauiPlainPureMode.Conveters
{
    public class ProxyTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null && value is ProxyType proxyType)
            {
                return proxyType.ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is not null && Enum.TryParse<ProxyType>(value.ToString(),out var proxyType))
            {
                return proxyType;
            }
            return null;
        }
    }
}
