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
    public class SpotifyController : ISpotifyController {

        /*
         SpotifyController uses code from https://github.com/ranveer5289/SpotifyNotifier-Windows and https://github.com/mscoolnerd/SpotifyLib
         */

        public event EventHandler SpotifyExited;
        public event EventHandler TrackChanged;
        public event EventHandler TrackStatusChanged;
        public event EventHandler SpotifyOpened;
        public event EventHandler TokenUpdated;

        private const string SpotifyRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\Spotify";

        private readonly ILog _Logger;
        private readonly ISpotifyLocalApi _localApi;
        private readonly ISpotifyWebApi _spotifyWebApi;

        private Process SpotifyProcess
        {
            get { return _spotifyNativeApi.SpotifyProcess; }
            set { _spotifyNativeApi.SpotifyProcess = value; }
        }
        private Thread _BackgroundChangeTracker;
        private Timer _songStatusWatcher;
        private Status _CurrentTrackInfo;
        private ISpotifyNativeApi _spotifyNativeApi;

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
            if (_CurrentTrackInfo != null && _CurrentTrackInfo.playing)
            {
                _CurrentTrackInfo.playing_position = (int)_CurrentTrackInfo.playing_position + 1;
                OnTrackTimerChanged();
            }
        }

        private void JoinBackgroundProcess()
        {
            if (_BackgroundChangeTracker != null && _BackgroundChangeTracker.IsAlive)
                return;
            
            _BackgroundChangeTracker = new Thread(() =>
            {
                BackgroundChangeTrackerWork();
            }) { IsBackground = true };
            _BackgroundChangeTracker.Start();
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
                    _CurrentTrackInfo = null;
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
        private async void BackgroundChangeTrackerWork() {
            try
            {
                while (true)
                {
                    if (SpotifyProcess != null)
                    {
                        //TODO this should be a bool field probably
                        int timeout = _CurrentTrackInfo == null ? -1 : 30;
                        Status newTrackInfo;
                        _localRequestTokenSource = new CancellationTokenSource(timeout == -1 ? 1000 : 45000);
                        try
                        {
                            _Logger.Info("Started retrieving information from spotify, timeout is" + timeout);
                            newTrackInfo =
                                await _localApi.SendLocalStatusRequest(true, true, _localRequestTokenSource.Token, timeout);
                            _Logger.Info("Finished retrieving information from spotify");
                        }
                        catch (TaskCanceledException)
                        {
                            _Logger.Info("Retrieving cancelled");
                            _CurrentTrackInfo = null;
                            //TODO this is bad and dirt
                            //nothing to do here
                            //if task was cancelled (e.g. spotify exited, just move on and wait for further stuff
                            continue;
                        }
                        finally
                        {
                            var tokenSource = _localRequestTokenSource;
                            _localRequestTokenSource = null;
                            tokenSource.Dispose();
                        }
                        if (newTrackInfo == null)
                        {
                            //TODO not sure when it happens, should probably happen never
                            throw new ApplicationException("Unable to retrieve track info");
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
                                    _CurrentTrackInfo = null;
                                    continue;
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
                            _Logger.WarnException("Failed to retrieve trackinfo", exc);
                            _CurrentTrackInfo = null;
                            Thread.Sleep(1000);
                            continue;
                        }

                        if (newTrackInfo.track == null)
                        {
                            _CurrentTrackInfo = null;
                            continue;
                        }

                        ProcessTrackInfo(newTrackInfo);
                    }
                    else
                    {
                        Thread.Sleep(1000);
                        //wait for spotify to reopen
                        await AttachToProcess();
                        if (SpotifyProcess != null)
                        {
                            OnSpotifyOpenend();
                        }
                    }

                }
            }
            catch (Exception exc) {
                _Logger.FatalException("BackgroundChangeTrackerWork failed with: "+ exc.Message, exc);
            }
        }

        protected internal void ProcessTrackInfo(Status newTrackInfo)
        {
            if (newTrackInfo == null)
            {
                return;
            }
            var newTrackInfoUri = newTrackInfo.track?.track_resource?.uri;
            if (_CurrentTrackInfo?.track?.track_resource == null || _CurrentTrackInfo.track.track_resource.uri
                != newTrackInfoUri)
            {
                _CurrentTrackInfo = newTrackInfo;
                OnTrackChanged();
                _songStatusWatcher?.Change(
                    GetDelayForPlaybackUpdate(newTrackInfo.playing_position), 1000);
            }
            else
            {
                _CurrentTrackInfo = newTrackInfo;
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

        public bool IsSpotifyInstalled() {
            try {
                //first try: the installation directory
                var spotifyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                               "Spotify", "spotify.exe");
                if (File.Exists(spotifyPath))
                    return true;

                //second try: look into the registry
                var registryKey = Registry.CurrentUser.OpenSubKey(SpotifyRegistryKey, false);
                if (registryKey != null && File.Exists((string) registryKey.GetValue("DisplayIcon", string.Empty)))
                    return true; //looks good, return true

                return false;
            }
            catch (Exception exc) {
                _Logger.WarnException("Failed to detect if Spotify is installed or not :(", exc);
                //In case of an error it's better to return true instead of false, because this makes MiniPie unusable if there is something wrong with Windows.
                return true;
            }
        }

        public Status GetStatus() {
            return _CurrentTrackInfo;
        }

        public async void Pause()
        {
            if(_CurrentTrackInfo.playing)
                await _localApi.Pause();
        }

        public async void Play()
        {
            if (!_CurrentTrackInfo.playing)
            {
                await _localApi.Resume();
            }
        }

        public async void PausePlay()
        {
            if (_CurrentTrackInfo.playing)
            {
                await _localApi.Pause();
            }
            else
            {
                await _localApi.Resume();
            }
        }

        public void NextTrack()
        {
            _spotifyNativeApi.NextTrack();
        }

        public void PreviousTrack()
        {
            _spotifyNativeApi.PreviousTrack();
        }

        public void VolumeUp()
        {
            _spotifyNativeApi.VolumeUp();
        }

        public void VolumeDown()
        {
            _spotifyNativeApi.VolumeDown();
        }

        public void OpenSpotify()
        {
            _spotifyNativeApi.OpenSpotify();
        }


        public void AttachTrackChangedHandler(EventHandler handler)
        {
            TrackChanged += handler;
            OnTrackChanged();
        }

        public void AttachTrackStatusChangedHandler(EventHandler handler)
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
            if(_BackgroundChangeTracker != null && _BackgroundChangeTracker.IsAlive)
                _BackgroundChangeTracker.Abort();
        }
    }
}
