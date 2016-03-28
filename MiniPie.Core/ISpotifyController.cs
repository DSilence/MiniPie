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
        Status GetStatus();
        Task Pause();
        Task Play();
        Task PausePlay();
        Task NextTrack();
        Task PreviousTrack();
        Task VolumeUp();
        Task VolumeDown();
        void OpenSpotify();
        void AttachTrackChangedHandler(EventHandler<EventArgs> handler);
        void AttachTrackStatusChangedHandler(EventHandler<EventArgs> handler);
        Task<bool> IsUserLoggedIn();
        Uri BuildLoginQuery();
        void Logout();
        Task<IList<Playlist>> GetPlaylists();
        Task AddToPlaylist(string playlistId, string trackUrls);
        Task<IList<SpotifyWeb.Models.Track>> GetTrackInfo(IList<string> trackIds);
        Task AddToQueue(IList<string> songUrls);
        Task<IList<bool>> IsTracksSaved(IList<string> trackIds);
        Task AddToMyMusic(IList<string> trackIds);
        Task RemoveFromMyMusic(IList<string> trackIds);
    }
}