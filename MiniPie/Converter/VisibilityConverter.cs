using System;
using System.Windows;
using System.Windows.Data;

namespace MiniPie.Converter {
    public sealed class VisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is bool)
                return ((bool) value) ? Visibility.Visible : Visibility.Collapsed;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return null;
        }
    }
}
