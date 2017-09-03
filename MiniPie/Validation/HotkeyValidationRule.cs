using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using MiniPie.Core.SpotifyNative.HotKeyManager;

namespace MiniPie.Validation
{
    public class HotkeyValidationRule: ValidationRule
    {
        public KeyManagerWrapper KeyManager { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, null);
            }
            if (value is KeyValuePair<Key, KeyModifier>)
            {
                var key = (KeyValuePair<Key, KeyModifier>)value;
                if (KeyManager.KeyManager.TestKeyValue(key))
                {
                    return new ValidationResult(true, null);
                }
                else
                {
                    return new ValidationResult(false, Properties.Resources.Settings_FailedToRegisterHotkey);
                }
            }
            return new ValidationResult(false, null);
        }
    }
}