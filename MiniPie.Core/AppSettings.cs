using System;
using System.Collections.Generic;
using Caliburn.Micro;
using MiniPie.Core.Enums;
using MiniPie.Core.Extensions;
using MiniPie.Core.SpotifyWeb;
using Newtonsoft.Json;

namespace MiniPie.Core {
    [JsonObject]
    public sealed class AppSettings : PropertyChangedBase {

        public AppSettings() {
            Positions = new List<WindowPosition>();
            ReadBroadcastMessageIds = new List<string>();
            UniqueApplicationIdentifier = Guid.NewGuid().ToString();
            ApplicationSize = ApplicationSize.Medium;
        }

        private bool _AlwaysOnTop;
        [JsonProperty]
        public bool AlwaysOnTop {
            get { return _AlwaysOnTop; }
            set { _AlwaysOnTop = value; NotifyOfPropertyChange(); }
        }

        private bool _StartWithWindows;
        [JsonProperty]
        public bool StartWithWindows {
            get { return _StartWithWindows; }
            set { _StartWithWindows = value; NotifyOfPropertyChange(); }
        }

        private bool _HideIfSpotifyClosed;
        [JsonProperty]
        public bool HideIfSpotifyClosed {
            get { return _HideIfSpotifyClosed; }
            set { _HideIfSpotifyClosed = value; NotifyOfPropertyChange(); }
        }

        private bool _DisableAnimations;
        [JsonProperty]
        public bool DisableAnimations {
            get { return _DisableAnimations; }
            set { _DisableAnimations = value; NotifyOfPropertyChange(); }
        }

        private Language _language;
        [JsonProperty]
        public Language Language
        {
            get { return _language; }
            set
            {
                _language = value;
                NotifyOfPropertyChange();
            }
        }

        public List<WindowPosition> Positions { get; set; }
        public List<string> ReadBroadcastMessageIds { get; set; }
        public string UniqueApplicationIdentifier { get; set; }

        private ApplicationSize _ApplicationSize;
        [JsonProperty]
        public ApplicationSize ApplicationSize {
            get { return _ApplicationSize; }
            set { _ApplicationSize = value; NotifyOfPropertyChange(); }
        }

        private bool _HotKeysEnabled;
        [JsonProperty]
        public bool HotKeysEnabled
        {
            get { return _HotKeysEnabled; }
            set { _HotKeysEnabled = value; NotifyOfPropertyChange(); }
        }

        private HotKeys _HotKeys;
        [JsonProperty]
        public HotKeys HotKeys
        {
            get { return _HotKeys; }
            set
            {
                _HotKeys = value; 
                NotifyOfPropertyChange();
            }
        }

        private bool _startMinimized;

        [JsonProperty]
        public bool StartMinimized
        {
            get { return _startMinimized; }
            set
            {
                _startMinimized = value;
                NotifyOfPropertyChange();
            }
        }

        private string _cacheFolder;

        [JsonProperty]
        public string CacheFolder
        {
            get { return _cacheFolder; }
            set
            {
                _cacheFolder = value;
                NotifyOfPropertyChange();
            }
        }

        private Token _spotifyToken;

        [JsonProperty]
        public Token SpotifyToken
        {
            get { return _spotifyToken; }
            set
            {
                _spotifyToken = value;
                NotifyOfPropertyChange();
            }
        }
    }
}