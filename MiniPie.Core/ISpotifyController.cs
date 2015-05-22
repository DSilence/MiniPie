using System;
using MiniPie.Core.SpotifyLocal;

namespace MiniPie.Core {
    public interface ISpotifyController : IDisposable {
        event EventHandler TrackChanged;
        event EventHandler SpotifyOpened;
        event EventHandler SpotifyExited;
        bool IsSpotifyOpen();
        bool IsSpotifyInstalled();
        string GetSongName();
        string GetArtistName();
        Status GetStatus();
        void PausePlay();
        void NextTrack();
        void PreviousTrack();
        void VolumeUp();
        void VolumeDown();
        void MaximizeSpotify();
    }
}