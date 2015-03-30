using System.Collections.Generic;
using System.Windows.Input;
using MiniPie.Core.HotKeyManager;

namespace MiniPie.Core
{
    public class HotKeys
    {
        public KeyValuePair<Key, KeyModifier> PlayPause { get; set; }
        public KeyValuePair<Key, KeyModifier> VolumeDown { get; set; }
        public KeyValuePair<Key, KeyModifier> VolumeUp { get; set; }
        public KeyValuePair<Key, KeyModifier> Next { get; set; }
        public KeyValuePair<Key, KeyModifier> Previous { get; set; }
    }
}
