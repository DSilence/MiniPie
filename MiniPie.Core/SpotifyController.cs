using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
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

        public ISubject<object> SpotifyExited { get;} = new Subject<object>();
        public ISubject<EventArgs> TrackChanged { get;} = new Subject<EventArgs>();
        public ISubject<EventArgs> TrackStatusChanged { get;} = new Subject<EventArgs>();
        public ISubject<object> SpotifyOpened { get;} = new Subject<object>();
        public ISubject<object> TokenUpdated { get;} = new Subject<object>();
        public ISubject<bool> LoggedInStatusChanged { get;} = new Subject<bool>();

        private readonly ILog _logger;
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
        protected internal int BackgroundDelay = 3000;

        public SpotifyController(ILog logger, ISpotifyLocalApi localApi, ISpotifyWebApi spotifyWebApi, ISpotifyNativeApi spotifyNativeApi) {
            _logger = logger;
            _localApi = localApi;
            _spotifyWebApi = spotifyWebApi;
            _spotifyNativeApi = spotifyNativeApi;
        }

        public async Task Initialize()
        {
            await _spotifyNativeApi.AttachToProcess(ProcessFound, ProcessExited);
            _songStatusWatcher = new Timer(SongTimerChanging, null, 1000, 1000);
            JoinBackgroundProcess();
            _spotifyWebApi.TokenUpdated += (sender, args) =>
            {
                TokenUpdated?.OnNext(args);
            };
        }

        private void ProcessExited(Process process)
        {
            CurrentTrackInfo = null;
            _localRequestTokenSource?.Cancel();
            OnSpotifyExited();
        }

        private async Task ProcessFound(Process process)
        {
            await _localApi.RenewToken().ConfigureAwait(false);
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

#pragma warning disable RECS0135 // Function does not reach its end or a 'return' statement by any of possible execution paths
            _backgroundChangeTracker = Task.Run(async () =>
#pragma warning restore RECS0135 // Function does not reach its end or a 'return' statement by any of possible execution paths
            {
                while (true)
                {
                    await BackgroundChangeTrackerWork().ConfigureAwait(false);
                }
            }, _backgroundChangeWorkerTokenSource.Token);
        }

        

        protected virtual void OnSpotifyExited() {
            SpotifyExited?.OnNext(EventArgs.Empty);
        }

        protected virtual void OnTrackChanged() {
            TrackChanged?.OnNext(EventArgs.Empty);
        }

        protected virtual void OnTrackTimerChanged()
        {
            TrackStatusChanged?.OnNext(EventArgs.Empty);
        }

        protected virtual void OnSpotifyOpenend() {
            SpotifyOpened?.OnNext(EventArgs.Empty);
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
                        _logger.Info("Started retrieving information from spotify, timeout is" + timeout);
                        newTrackInfo =
                            await
                                _localApi.SendLocalStatusRequest(true, true, _localRequestTokenSource.Token, timeout).ConfigureAwait(false);
                        _logger.Info("Finished retrieving information from spotify");
                    }
                    catch (TaskCanceledException)
                    {
                        _logger.Info("Retrieving cancelled");
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
                        _logger.Warn("Failed to retrieve track info. Track info is empty");
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
                                _logger.Info("Renew updateToken and try again");
                                await _localApi.RenewToken();
                                if (_localApi.HasValidToken)
                                {
                                    throw new Exception("Failed to renew local api token");
                                }
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
                        _logger.WarnException("Failed to retrieve trackinfo: " + exc.Message, exc);
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
                    await _spotifyNativeApi.AttachToProcess(ProcessFound, ProcessExited).ConfigureAwait(false);
                    if (SpotifyProcess != null)
                    {
                        OnSpotifyOpenend();
                    }
                }
            }
            catch (Exception exc)
            {
                _logger.FatalException("BackgroundChangeTrackerWork failed with: " + exc.Message, exc);
                _logger.Fatal(exc.StackTrace);
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
            if (!IsSpotifyOpen())
            {
                return Task.FromResult(false);
            }
            if (!CurrentTrackInfo.playing)
            {
                return _localApi.Resume();
            }
            return Task.FromResult(false);
        }

        public Task PausePlay()
        {
            if (!IsSpotifyOpen())
            {
                return Task.FromResult(false);
            }
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
            if (!IsSpotifyOpen())
            {
                return Task.FromResult(false);
            }
            _spotifyNativeApi.NextTrack();
            return Task.FromResult(false);
        }

        public Task PreviousTrack()
        {
            if (!IsSpotifyOpen())
            {
                return Task.FromResult(false);
            }
            _spotifyNativeApi.PreviousTrack();
            return Task.FromResult(false);
        }

        public async Task VolumeUp()
        {
            var currentVolume = await GetVolume();
            var targetVolume = currentVolume + 0.1;
            await SetSpotifyVolume(targetVolume);
        }

        public async Task VolumeDown()
        {
            var currentVolume = await GetVolume();
            var targetVolume = currentVolume - 0.1;
            await SetSpotifyVolume(targetVolume);
        }

        public void OpenSpotify()
        {
            _spotifyNativeApi.OpenSpotify();
        }

        public void AttachTrackChangedHandler(Action<EventArgs> handler)
        {
            TrackChanged.Subscribe(handler, CancellationToken.None);
            OnTrackChanged();
        }

        public void AttachTrackStatusChangedHandler(Action<EventArgs> handler)
        {
            TrackStatusChanged.Subscribe(handler, CancellationToken.None);
        }

        public async Task<bool> IsUserLoggedIn()
        {
            User user;
            try
            {
                _logger.Info("Verifying Login Status");
                user = await _spotifyWebApi.GetProfile();
            }
            catch (HttpRequestException e)
            {
                _logger.WarnException("User login failed", e);
                return false;
            }
            bool result = user != null;
            _logger.Info($"User verifying result is:{result}");
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

        private async Task<double> GetVolume()
        {
            var status = await _localApi.SendLocalStatusRequest(true, true, CancellationToken.None);
            return status.volume;
        }

        public async Task<double> SetSpotifyVolume(double volume)
        {
            try
            {
                var newVolume = (int)(volume * 100); 
                await _spotifyWebApi.SetVolume(newVolume);
                return newVolume;
            }
            catch(Exception e)
            {
                _logger.FatalException("Failed to change volume with " + e.Message, e);
            }
            return double.NaN;
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
