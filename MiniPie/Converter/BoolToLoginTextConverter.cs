using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MiniPie.Converter
{
    public class BoolToLoginTextConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool) value)
            {
                return Properties.Resources.Settings_Logout;
            }
            else
            {
                return Properties.Resources.Settings_Login;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
