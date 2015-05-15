using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using MiniPie.Core.HotKeyManager;

namespace MiniPie.Converter
{
    public class KeyCodeToReadableStringConverter: IValueConverter
    {
        private const string FormatString = "{0} + {1}";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is KeyValuePair<Key, KeyModifier>))
                return null;
            var keyValuePairValue = (KeyValuePair<Key, KeyModifier>) value;
                //TODO probably could localize it, but nobody cares for now
            return keyValuePairValue.Key.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
