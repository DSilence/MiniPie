using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using MiniPie.Core.SpotifyLocal;
using MiniPie.Core.SpotifyNative;
using MiniPie.Core.SpotifyWeb;
using MiniPie.Core.SpotifyWeb.Models;
using Timer = System.Threading.Timer;

namespace MiniPie.Core {
    public class SpotifyController: ISpotifyController {

        /*
         SpotifyController uses code from https://github.com/ranveer5289/SpotifyNotifier-Windows and https://github.com/mscoolnerd/SpotifyLib
         */

        public event EventHandler SpotifyExited;
        public event EventHandler<EventArgs> TrackChanged;
        public event EventHandler<EventArgs> TrackStatusChanged;
        public event EventHandler SpotifyOpened;
        public event EventHandler TokenUpdated;

        private const string SpotifyRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\Spotify";

        private readonly ILog _Logger;
        private readonly ISpotifyLocalApi _localApi;
        private readonly ISpotifyWebApi _spotifyWebApi;

        internal Process SpotifyProcess
        {
            get { return _spotifyNativeApi.SpotifyProcess; }
            set { _spotifyNativeApi.SpotifyProcess = value; }
        }
        private Task _backgroundChangeTracker;
        private readonly CancellationTokenSource _backgroundChangeWorkerTokenSource = new CancellationTokenSource();
        private Timer _songStatusWatcher;
        protected internal Status CurrentTrackInfo;
        private readonly ISpotifyNativeApi _spotifyNativeApi;
        protected internal int BackgroundDelay = 1000;

        public SpotifyController(ILog logger, ISpotifyLocalApi localApi, ISpotifyWebApi spotifyWebApi, ISpotifyNativeApi spotifyNativeApi) {
            _Logger = logger;
            _localApi = localApi;
            _spotifyWebApi = spotifyWebApi;
            _spotifyNativeApi = spotifyNativeApi;
        }

        public async Task Initialize()
        {
            await AttachToProcess();
            _songStatusWatcher = new Timer(SongTimerChanging, null, 1000, 1000);
            JoinBackgroundProcess();
            _spotifyWebApi.TokenUpdated += (sender, args) =>
            {
                if (TokenUpdated != null)
                {
                    TokenUpdated(sender, args);
                }
            };
        }

        private void SongTimerChanging(object state)
        {
            //every second increase the track time by 1
            if (CurrentTrackInfo != null && CurrentTrackInfo.playing)
            {
                CurrentTrackInfo.playing_position = (int)CurrentTrackInfo.playing_position + 1;
                OnTrackTimerChanged();
            }
        }

        private void JoinBackgroundProcess()
        {
            if (_backgroundChangeTracker != null && !_backgroundChangeTracker.IsCompleted)
                return;
            
            _backgroundChangeTracker = Task.Run(async () =>
            {
                while (true)
                {
                    await BackgroundChangeTrackerWork().ConfigureAwait(false);
                }
                
            }, _backgroundChangeWorkerTokenSource.Token);
        }

        private async Task AttachToProcess() {
            SpotifyProcess = null;
            SpotifyProcess = Process.GetProcessesByName("spotify")
                .FirstOrDefault(p => !string.IsNullOrEmpty(p.MainWindowTitle));
            if (SpotifyProcess != null)
            {
                //Renew updateToken for Spotify local api
                await _localApi.RenewToken().ConfigureAwait(false);
                SpotifyProcess.EnableRaisingEvents = true;
                SpotifyProcess.Exited += (o, e) =>
                {
                    SpotifyProcess = null;
                    CurrentTrackInfo = null;
                    _localRequestTokenSource?.Cancel();
                    OnSpotifyExited();
                };
            }
        }

        protected virtual void OnSpotifyExited() {
            var handler = SpotifyExited;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnTrackChanged() {
            var handler = TrackChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnTrackTimerChanged()
        {
            var handler = TrackStatusChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnSpotifyOpenend() {
            var handler = SpotifyOpened;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private CancellationTokenSource _localRequestTokenSource;
        //TODO code dupes
        internal async Task BackgroundChangeTrackerWork()
        {
            try
            {
                if (SpotifyProcess != null)
                {
                    //TODO this should be a bool field probably
                    int timeout = CurrentTrackInfo == null ? -1 : 30;
                    Status newTrackInfo;
                    _localRequestTokenSource = new CancellationTokenSource(timeout == -1 ? 1000 : 45000);
                    try
                    {
                        _Logger.Info("Started retrieving information from spotify, timeout is" + timeout);
                        newTrackInfo =
                            await
                                _localApi.SendLocalStatusRequest(true, true, _localRequestTokenSource.Token, timeout).ConfigureAwait(false);
                        _Logger.Info("Finished retrieving information from spotify");
                    }
                    catch (TaskCanceledException)
                    {
                        _Logger.Info("Retrieving cancelled");
                        CurrentTrackInfo = null;
                        //TODO this is bad and dirt
                        //nothing to do here
                        //if task was cancelled (e.g. spotify exited, just move on and wait for further stuff
                        return;
                    }
                    finally
                    {
                        var tokenSource = _localRequestTokenSource;
                        _localRequestTokenSource = null;
                        tokenSource.Dispose();
                    }
                    if (newTrackInfo == null)
                    {
                        _Logger.Warn("Failed to retrieve track info. Track info is empty");
                        return;
                    }
                    try
                    {
                        if (newTrackInfo.error != null)
                        {
                            if (newTrackInfo.error.message.Contains("Invalid Csrf") ||
                                newTrackInfo.error.message.Contains("OAuth updateToken") ||
                                newTrackInfo.error.message.Contains("Expired OAuth localRequestToken"))
                            {
                                //try to renew updateToken and retrieve status again
                                _Logger.Info("Renew updateToken and try again");
                                await _localApi.RenewToken();
                                CurrentTrackInfo = null;
                                return;
                            }
                            else
                            {
                                throw new Exception(string.Format("Spotify API error: {0}",
                                    newTrackInfo.error.message));
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        //TODO this should crash the application
                        _Logger.WarnException("Failed to retrieve trackinfo: " + exc.Message, exc);
                        CurrentTrackInfo = null;
                        Thread.Sleep(BackgroundDelay);
                        return;
                    }

                    if (newTrackInfo.track == null)
                    {
                        CurrentTrackInfo = null;
                        return;
                    }

                    ProcessTrackInfo(newTrackInfo);
                }
                else
                {
                    Thread.Sleep(BackgroundDelay);
                    //wait for spotify to reopen
                    await AttachToProcess();
                    if (SpotifyProcess != null)
                    {
                        OnSpotifyOpenend();
                    }
                }
            }
            catch (Exception exc)
            {
                _Logger.FatalException("BackgroundChangeTrackerWork failed with: " + exc.Message, exc);
                _Logger.Fatal(exc.StackTrace);
            }
        }

        protected internal void ProcessTrackInfo(Status newTrackInfo)
        {
            if (newTrackInfo == null)
            {
                return;
            }
            var newTrackInfoUri = newTrackInfo.track?.track_resource?.uri;
            if (CurrentTrackInfo?.track?.track_resource == null || CurrentTrackInfo.track.track_resource.uri
                != newTrackInfoUri)
            {
                CurrentTrackInfo = newTrackInfo;
                OnTrackChanged();
                _songStatusWatcher?.Change(
                    GetDelayForPlaybackUpdate(newTrackInfo.playing_position), 1000);
            }
            else
            {
                CurrentTrackInfo = newTrackInfo;
                OnTrackTimerChanged();
                _songStatusWatcher?.Change(
                    GetDelayForPlaybackUpdate(newTrackInfo.playing_position), 1000);
            }
        }

        protected internal int GetDelayForPlaybackUpdate(double playPosition)
        {
            int i = (int) playPosition;
            double fract = playPosition - i;
            int result =  (int) (1000 - fract*1000);
            return result;
        }

        public bool IsSpotifyOpen() {
            return SpotifyProcess != null;
        }

        public Status GetStatus() {
            return CurrentTrackInfo;
        }

        public Task Pause()
        {
            if (CurrentTrackInfo.playing)
            {
                return _localApi.Pause();
            }
            return Task.FromResult(false);
        }

        public Task Play()
        {
            if (!CurrentTrackInfo.playing)
            {
                return _localApi.Resume();
            }
            return Task.FromResult(false);
        }

        public Task PausePlay()
        {
            if (CurrentTrackInfo.playing)
            {
                return _localApi.Pause();
            }
            else
            {
                return _localApi.Resume();
            }
        }

        public Task NextTrack()
        {
            _spotifyNativeApi.NextTrack();
            return Task.FromResult(false);
        }

        public Task PreviousTrack()
        {
            _spotifyNativeApi.PreviousTrack();
            return Task.FromResult(false);
        }

        public Task VolumeUp()
        {
            _spotifyNativeApi.VolumeUp();
            return Task.FromResult(false);
        }

        public Task VolumeDown()
        {
            _spotifyNativeApi.VolumeDown();
            return Task.FromResult(false);
        }

        public void OpenSpotify()
        {
            _spotifyNativeApi.OpenSpotify();
        }

        public void AttachTrackChangedHandler(EventHandler<EventArgs> handler)
        {
            TrackChanged += handler;
            OnTrackChanged();
        }

        public void AttachTrackStatusChangedHandler(EventHandler<EventArgs> handler)
        {
            TrackStatusChanged += handler;
        }

        public async Task<bool> IsUserLoggedIn()
        {
            User user;
            try
            {
                _Logger.Info("Verifying Login Status");
                user = await _spotifyWebApi.GetProfile();
            }
            catch (HttpRequestException e)
            {
                _Logger.WarnException("User login failed", e);
                return false;
            }
            bool result = user != null;
            _Logger.Info(string.Format("User verifying result is:{0}", result));
            return result;
        }

        public Uri BuildLoginQuery()
        {
            return _spotifyWebApi.BuildLoginQuery();
        }

        public void Logout()
        {
            _spotifyWebApi.Logout();
        }

        public async Task<IList<Playlist>> GetPlaylists()
        {
            return await _spotifyWebApi.GetUserPlaylists();
        }

        public async Task AddToPlaylist(string playlistId, string trackUrls)
        {
            await _spotifyWebApi.AddToPlaylist(playlistId, trackUrls);
        }

        public async Task<IList<SpotifyWeb.Models.Track>> GetTrackInfo(IList<string> trackIds)
        {
            return await _spotifyWebApi.GetTrackInfo(trackIds);
        }

        public async Task AddToQueue(IList<string> songUrls)
        {
            foreach (var song in songUrls)
            {
                await _localApi.Queue(song);
            }
        }

        public async Task<IList<bool>> IsTracksSaved(IList<string> trackIds)
        {
            return await _spotifyWebApi.IsTracksSaved(trackIds);
        }

        public async Task AddToMyMusic(IList<string> trackIds)
        {
            await _spotifyWebApi.AddToMyMusic(trackIds);
        }

        public async Task RemoveFromMyMusic(IList<string> trackIds)
        {
            await _spotifyWebApi.RemoveFromMyMusic(trackIds);
        }

        public void Dispose()
        {
            _songStatusWatcher?.Dispose();
            if(_backgroundChangeTracker != null && _backgroundChangeTracker.IsCompleted)
                _backgroundChangeWorkerTokenSource.Cancel();

            _backgroundChangeWorkerTokenSource.Dispose();
        }
    }
}
