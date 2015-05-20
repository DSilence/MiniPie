using System;
using System.Collections.Generic;
using System.Windows.Input;
using MiniPie.Core.HotKeyManager;

namespace MiniPie.Core
{
    public class HotKeys: ICloneable
    {
        public KeyValuePair<Key, KeyModifier> PlayPause { get; set; }
        public KeyValuePair<Key, KeyModifier> VolumeDown { get; set; }
        public KeyValuePair<Key, KeyModifier> VolumeUp { get; set; }
        public KeyValuePair<Key, KeyModifier> Next { get; set; }
        public KeyValuePair<Key, KeyModifier> Previous { get; set; }

        public object Clone()
        {
            return new HotKeys
            {
                PlayPause = new KeyValuePair<Key, KeyModifier>(PlayPause.Key, PlayPause.Value),
                VolumeDown = new KeyValuePair<Key, KeyModifier>(VolumeDown.Key, VolumeDown.Value),
                VolumeUp = new KeyValuePair<Key, KeyModifier>(VolumeUp.Key, VolumeUp.Value),
                Next = new KeyValuePair<Key, KeyModifier>(Next.Key, Next.Value),
                Previous = new KeyValuePair<Key, KeyModifier>(Previous.Key, Previous.Value)
            };
        }
    }
}
