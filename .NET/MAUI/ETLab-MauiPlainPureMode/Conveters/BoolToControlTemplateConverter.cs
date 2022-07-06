﻿using ETLab_MauiPlainPureMode.Pages;
using System.Globalization;

namespace ETLab_MauiPlainPureMode.Conveters
{
    public class BoolToControlTemplateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var socks5IsChecked = (bool)value;
            var natCheckPage = parameter as NATCheckPage;

            if (socks5IsChecked)
            {
                //return (ControlTemplate)App.Current.Resources["RFC5780ControlTemplate"];
                return (ControlTemplate)natCheckPage.Resources["bar"];
            }
            else
            {
                //return (ControlTemplate)App.Current.Resources["RFC3489ControlTemplate"];
                return (ControlTemplate)natCheckPage.Resources["foo"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
