using System;
using System.Windows;
using System.Windows.Data;

namespace MiniPie.Converter {
    public sealed class VisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts bool field to visibility
        /// If you want to inverse the value, use parameter
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is bool)
            {
                bool inversed = false;
                if (parameter is bool)
                {
                    inversed = (bool) parameter;
                }
                if (inversed)
                {
                    return ((bool)value) ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
                }
                
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return null;
        }
    }
}
