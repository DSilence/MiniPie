using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MiniPie.Core.SpotifyNative.HotKeyManager
{
    public class KeyManager: IDisposable
    {
        private readonly ISpotifyController _spotifyController;
        private readonly HotKey _next;
        private readonly HotKey _previous;
        private readonly HotKey _playPause;
        private readonly HotKey _volumeUp;
        private readonly HotKey _volumeDown;
        private readonly ILog _log;

        public KeyManager(ISpotifyController spotifyController, ILog log)
        {
            _spotifyController = spotifyController;
            _next = new HotKey(Key.None, KeyModifier.None, 
                key => _spotifyController.NextTrack());
            _previous = new HotKey(Key.None, KeyModifier.None, 
                key => _spotifyController.PreviousTrack());
            _playPause = new HotKey(Key.None, KeyModifier.None,
                key => _spotifyController.PausePlay());
            _volumeDown = new HotKey(Key.None, KeyModifier.None,
                key => _spotifyController.VolumeDown());
            _volumeUp = new HotKey(Key.None, KeyModifier.None, 
                key => _spotifyController.VolumeUp());
            _log = log;

        }

        public Task RegisterHotKeysAsync(HotKeys hotKeys)
        {
            return Task.Run(() => RegisterHotKeys(hotKeys));
        }

        public Task UnregisterHotKeysAsync()
        {
            return Task.Run((Action) UnregisterHotKeys);
        }

        public void RegisterHotKeys(HotKeys hotKeys)
        {
            PopulateKeyAndModifier(_next, hotKeys.Next);
            PopulateKeyAndModifier(_previous, hotKeys.Previous);
            PopulateKeyAndModifier(_playPause, hotKeys.PlayPause);
            PopulateKeyAndModifier(_volumeDown, hotKeys.VolumeDown);
            PopulateKeyAndModifier(_volumeUp, hotKeys.VolumeUp);
        }

        private void PopulateKeyAndModifier(
            HotKey hotKey, KeyValuePair<Key, KeyModifier> value, bool update = true)
        {
            if (update)
            {
                hotKey.Unregister();
            }
            hotKey.Key = value.Key;
            hotKey.KeyModifiers = value.Value;
            if (update)
            {
                if (!hotKey.Register())
                {
                    _log.Warn(string.Format("Failed to register hotkey:{0},{1}", 
                        value.Key, value.Value));
                }
            }
        }

        public bool TestKeyValue(KeyValuePair<Key, KeyModifier> value)
        {
            using (var hotkey = new HotKey(value.Key, value.Value, key => { }))
            {
                return hotkey.Register();
            }
        }

        public void UnregisterHotKeys()
        {
            _next.Unregister();
            _previous.Unregister();
            _playPause.Unregister();
            _volumeDown.Unregister();
            _volumeUp.Unregister();
        }

#pragma warning disable RECS0154 // Parameter is never used
        private void Dispose(bool disposing)
#pragma warning restore RECS0154 // Parameter is never used
        {
            _next.Dispose();
            _previous.Dispose();
            _playPause.Dispose();
            _volumeDown.Dispose();
            _volumeUp.Dispose();
            GC.SuppressFinalize(this);
        }
        public void Dispose()
        {
            Dispose(true);
        }
    }
}