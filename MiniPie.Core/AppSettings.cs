using System.Collections.Generic;
using MiniPie.Core.Enums;
using MiniPie.Core.SpotifyWeb;
using Newtonsoft.Json;

namespace MiniPie.Core {
    [JsonObject]
    public class AppSettings
    {
        
        [JsonProperty]
        public virtual bool AlwaysOnTop { get; set; }

        [JsonProperty]
        public virtual bool StartWithWindows { get; set; }

        [JsonProperty]
        public virtual bool HideIfSpotifyClosed { get; set; }

        [JsonProperty]
        public virtual bool DisableAnimations { get; set; }

        [JsonProperty]
        public virtual Language Language { get; set; } = LanguageHelper.English;

        public virtual List<WindowPosition> Positions { get; set; } = new List<WindowPosition>();

        [JsonProperty]
        public virtual ApplicationSize ApplicationSize { get; set; } = ApplicationSize.Medium;
        [JsonProperty]
        public virtual bool HotKeysEnabled { get; set; }
        [JsonProperty]
        public virtual HotKeys HotKeys { get; set; }
        [JsonProperty]
        public virtual bool StartMinimized { get; set; }
        [JsonProperty]
        public virtual string CacheFolder { get; set; }
        [JsonProperty]
        public virtual Token SpotifyToken { get; set; }
        [JsonProperty]
        public virtual LockScreenBehavior LockScreenBehavior { get; set; } = LockScreenBehavior.Disabled;
        [JsonProperty]
        public virtual UpdatePreference UpdatePreference { get; set; } = UpdatePreference.Stable;
        [JsonProperty]
        public virtual bool SingleClickHide { get; set; }
    }
}