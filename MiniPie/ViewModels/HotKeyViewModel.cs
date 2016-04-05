using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using MiniPie.Converter;
using MiniPie.Core;
using MiniPie.Core.SpotifyNative.HotKeyManager;

namespace MiniPie.ViewModels
{
    public class HotKeyViewModel : PropertyChangedBase
    {
        private readonly AppSettings _settings;
        private HotKeys _lastValidHotkeys;

        public HotKeyViewModel(KeyManager manager, AppSettings settings)
        {
            KeyManager = manager;
            _settings = settings;
            _lastValidHotkeys = HotKeys.Clone();
        }

        public HotKeys HotKeys
        {
            get
            {
                if (_settings.HotKeys == null)
                {
                    _settings.HotKeys = new HotKeys();
                }
                return _settings.HotKeys;
            }
            set
            {
                _settings.HotKeys = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(PlayPause));
                NotifyOfPropertyChange(nameof(Previous));
                NotifyOfPropertyChange(nameof(Next));
                NotifyOfPropertyChange(nameof(VolumeUp));
                NotifyOfPropertyChange(nameof(VolumeDown));
            }
        }

        public void PerformHotKeyUpdate()
        {
            try
            {
                UnregisterHotKeys();
                RegisterHotKeys();
                _lastValidHotkeys = HotKeys.Clone();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.Settings_InvalidHotkeys);
                UnregisterHotKeys();
                HotKeys = _lastValidHotkeys;
            }
        }

        public void UnregisterHotKeys()
        {
            KeyManager.UnregisterHotKeys();
        }

        public void RegisterHotKeys()
        {
            KeyManager.RegisterHotKeys(HotKeys);
        }

        public bool HotKeysEnabled
        {
            get { return _settings.HotKeysEnabled; }
            set
            {
                if (_settings.HotKeysEnabled != value)
                {
                    _settings.HotKeysEnabled = value;
                }
            }
        }

        #region HotKeys

        public KeyValuePair<Key, KeyModifier> PlayPause
        {
            get { return HotKeys.PlayPause; }
            set { HotKeys.PlayPause = value; }
        }

        public KeyValuePair<Key, KeyModifier> Previous
        {
            get { return HotKeys.Previous; }
            set { HotKeys.Previous = value; }
        }

        public KeyValuePair<Key, KeyModifier> Next
        {
            get { return HotKeys.Next; }
            set { HotKeys.Next = value; }
        }

        public KeyValuePair<Key, KeyModifier> VolumeUp
        {
            get { return HotKeys.VolumeUp; }
            set { HotKeys.VolumeUp = value; }
        }

        public KeyValuePair<Key, KeyModifier> VolumeDown
        {
            get { return HotKeys.VolumeDown; }
            set { HotKeys.VolumeDown = value; }
        }

        public void Clear(TextBox parameter)
        {
            parameter.Text = "";
            parameter.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }

        public KeyManager KeyManager { get; }

        #endregion
    }
}
