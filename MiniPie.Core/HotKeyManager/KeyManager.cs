using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MiniPie.Core.HotKeyManager
{
    public class KeyManager: IDisposable
    {
        private readonly ISpotifyController _spotifyController;
        private readonly HotKey _next;
        private readonly HotKey _previous;
        private readonly HotKey _playPause;
        private readonly HotKey _volumeUp;
        private readonly HotKey _volumeDown;

        public KeyManager(ISpotifyController spotifyController)
        {
            _spotifyController = spotifyController;
            _next = new HotKey(Key.None, KeyModifier.None, 
                key => _spotifyController.NextTrack(), false);
            _previous = new HotKey(Key.None, KeyModifier.None, 
                key => _spotifyController.PreviousTrack(), false);
            _playPause = new HotKey(Key.None, KeyModifier.None,
                key => _spotifyController.PausePlay(), false);
            _volumeDown = new HotKey(Key.None, KeyModifier.None,
                key => _spotifyController.VolumeDown(), false);
            _volumeUp = new HotKey(Key.None, KeyModifier.None, 
                key => _spotifyController.VolumeUp(), false);
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
                hotKey.Register();
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

        public void Dispose()
        {
            _next.Dispose();
            _previous.Dispose();
            _playPause.Dispose();
            _volumeDown.Dispose();
            _volumeUp.Dispose();
        }
    }
}