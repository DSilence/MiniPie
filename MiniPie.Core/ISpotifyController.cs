using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniPie.Core.SpotifyLocal;
using MiniPie.Core.SpotifyWeb.Models;

namespace MiniPie.Core {
    public interface ISpotifyController : IDisposable {
        event EventHandler SpotifyOpened;
        event EventHandler SpotifyExited;
        event EventHandler TokenUpdated;
        Task Initialize();
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
        void Logout();
        Task<IList<Playlist>> GetPlaylists();
        Task AddToPlaylist(string playlistId, string trackUrls);
        Task<IList<SpotifyWeb.Models.Track>> GetTrackInfo(IList<string> trackIds);
        Task AddToQueue(IList<string> songUrls);
    }
}