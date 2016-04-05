using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using MiniPie.Core.SpotifyNative.HotKeyManager;

namespace MiniPie.Converter
{
    public class KeyCodeToReadableStringConverter: IValueConverter
    {
        private KeyConverter _keyConverter = new KeyConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is KeyValuePair<Key, KeyModifier>))
                return null;
            var keyValuePairValue = (KeyValuePair<Key, KeyModifier>) value;
            string modifier = keyValuePairValue.Value == KeyModifier.None
                ? string.Empty
                : keyValuePairValue.Value.ToString();
            string key = keyValuePairValue.Key == Key.None ? string.Empty : keyValuePairValue.Key.ToString();
                //TODO probably could localize it, but nobody cares for now
            if (key == string.Empty)
            {
                return string.Empty;
            }
            return $"{modifier} + {key}";
        }

        public static KeyValuePair<Key, KeyModifier> ConvertBack(string stringValue)
        {
            if (stringValue == "")
            {
                return new KeyValuePair<Key, KeyModifier>();
            }

            string keyModifierString;
            string keyString;
            if (stringValue.IndexOf('+') == -1)
            {
                keyModifierString = KeyModifier.None.ToString();
                keyString = stringValue.Trim();
            }
            else
            {
                var splitted = stringValue.Split('+');
                keyString = splitted[1].Trim();
                keyModifierString = splitted[0].Trim();
            }

            var key = (Key)Enum.Parse(typeof(Key), keyString);
            var keyModifier = (KeyModifier)Enum.Parse(typeof(KeyModifier), keyModifierString);
            return new KeyValuePair<Key, KeyModifier>(key, keyModifier);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string))
            {
                return null;
            }
            var stringValue = (string) value;
            return ConvertBack(stringValue);
        }
    }
}
