using System;
using System.Threading.Tasks;
using MiniPie.Core.SpotifyLocal;

namespace MiniPie.Core {
    public interface ISpotifyController : IDisposable {
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
        void OpenSpotify();
        void AttachTrackChangedHandler(EventHandler handler);
        void AttachTrackStatusChangedHandler(EventHandler handler);
        Task<bool> IsUserLoggedIn();
        Uri BuildLoginQuery();
        Task UpdateToken(string token);
    }
}