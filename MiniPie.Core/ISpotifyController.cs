using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MiniPie.Core.SpotifyLocal;
using MiniPie.Core.SpotifyWeb.Models;

namespace MiniPie.Core {
    public interface ISpotifyController : IDisposable {
        ISubject<object> SpotifyExited { get; }
        ISubject<EventArgs> TrackChanged { get; }
        ISubject<EventArgs> TrackStatusChanged { get; }
        ISubject<object> SpotifyOpened { get; }
        ISubject<object> TokenUpdated { get; }
        ISubject<bool> LoggedInStatusChanged { get; }
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
        void AttachTrackChangedHandler(Action<EventArgs> handler);
        void AttachTrackStatusChangedHandler(Action<EventArgs> handler);
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
        Task<double> SetSpotifyVolume(double volume);
    }
}