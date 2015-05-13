using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Caliburn.Micro;
using MiniPie.Core;
using MiniPie.Core.HotKeyManager;

namespace MiniPie.ViewModels
{
    public class HotKeyViewModel: PropertyChangedBase
    {
        private readonly KeyManager _manager;
        private AppSettings _settings;
        private readonly object _lockObject = new object();

        public HotKeyViewModel(KeyManager manager, AppSettings settings)
        {
            this._manager = manager;
            this._settings = settings;
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
                PerformHotKeyUpdate();
            }
        }

        public void PerformHotKeyUpdate()
        {
            UnregisterHotKeys();
            RegisterHotKeys();
        }

        private void UnregisterHotKeys()
        {
            _manager.UnregisterHotKeys();
        }

        private void RegisterHotKeys()
        {
            _manager.RegisterHotKeys(HotKeys);
        }

        public bool HotKeysEnabled
        {
            get { return _settings.HotKeysEnabled; }
            set
            {
                if (_settings.HotKeysEnabled != value)
                {
                    _settings.HotKeysEnabled = value;
                    NotifyOfPropertyChange();
                    lock (_lockObject)
                    {
                        if (value)
                        {
                            RegisterHotKeys();
                        }
                        else
                        {
                            UnregisterHotKeys();
                        }
                    }
                }
            }
        }

        #region HotKeys

        public KeyValuePair<Key, KeyModifier> PlayPause
        {
            get { return HotKeys.PlayPause; }
            set
            {
                HotKeys.PlayPause = value;
                NotifyOfPropertyChange();
            }
        }

        public KeyValuePair<Key, KeyModifier> Previous
        {
            get { return HotKeys.Previous; }
            set
            {
                HotKeys.Previous = value;
                NotifyOfPropertyChange();
            }
        }

        public KeyValuePair<Key, KeyModifier> Next
        {
            get { return HotKeys.Next; }
            set
            {
                HotKeys.Next = value;
                NotifyOfPropertyChange();
            }
        }

        public KeyValuePair<Key, KeyModifier> VolumeUp
        {
            get { return HotKeys.VolumeUp; }
            set
            {
                HotKeys.VolumeUp = value;
                NotifyOfPropertyChange();
            }
        }

        public KeyValuePair<Key, KeyModifier> VolumeDown
        {
            get { return HotKeys.VolumeDown; }
            set
            {
                HotKeys.VolumeDown = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion
    }
}
