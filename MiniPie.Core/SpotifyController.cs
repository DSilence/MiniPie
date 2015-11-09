﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Win32;
using MiniPie.Core.SpotifyLocal;
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

        #region Win32Imports

        private const int SW_RESTORE = 9;
        private const int MINIMIZED_STATE = 2;


        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Point rcNormalPosition;
        }
        #endregion

        const int KeyMessage = 0x319;
        private const uint WM_COMMAND = 0x0111;

        private const long NexttrackKey = 0xB0000L;
        private const long PreviousKey = 0xC0000L;
        private const long VolumeUpKey = 0x10079L;
        private const long VolumeDownKey = 0x1007AL;

        private const string SpotifyRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\Spotify";

        private readonly ILog _Logger;
        private readonly ISpotifyLocalApi _localApi;
        private readonly ISpotifyWebApi _spotifyWebApi;

        private Process _SpotifyProcess;
        private Thread _BackgroundChangeTracker;
        private Timer _songStatusWatcher;
        private Status _CurrentTrackInfo;

        public SpotifyController(ILog logger, ISpotifyLocalApi localApi, ISpotifyWebApi spotifyWebApi) {
            _Logger = logger;
            _localApi = localApi;
            _spotifyWebApi = spotifyWebApi;
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
            _SpotifyProcess = null;
            _SpotifyProcess = Process.GetProcessesByName("spotify")
                .FirstOrDefault(p => p.MainWindowHandle.ToInt32() > 0);
            if (_SpotifyProcess != null)
            {
                //Renew updateToken for Spotify local api
                await _localApi.RenewToken();
                _SpotifyProcess.EnableRaisingEvents = true;
                _SpotifyProcess.Exited += (o, e) =>
                {
                    _SpotifyProcess = null;
                    _CurrentTrackInfo = null;
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

        //TODO code dupes
        private async void BackgroundChangeTrackerWork() {
            try
            {
                while (true)
                {
                    if (_SpotifyProcess != null)
                    {
                        //TODO this should be a bool field probably
                        int timeout = _CurrentTrackInfo == null ? -1 : 30;
                        Status newTrackInfo;
                        try
                        {
                            _Logger.Info("Started retrieving information from spotify, timeout is" + timeout);
                            newTrackInfo =
                                await _localApi.SendLocalStatusRequest(true, true, timeout);
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
                        if (newTrackInfo == null)
                        {
                            //TODO not sure when it happens, should probably happen never
                            throw new ApplicationException("Unable to retrieve track info");
                        }
                        try
                        {
                            if (newTrackInfo.error != null)
                            {
                                if ((newTrackInfo.error.message.Contains("Invalid Csrf") ||
                                     newTrackInfo.error.message.Contains("OAuth updateToken") ||
                                     newTrackInfo.error.message.Contains("Expired OAuth token")))
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
                        if (_SpotifyProcess != null)
                        {
                            OnSpotifyOpenend();
                        }
                    }

                }
            }
            catch (Exception exc) {
                _Logger.FatalException("BackgroundChangeTrackerWork failed", exc);
            }
        }

        protected internal void ProcessTrackInfo(Status newTrackInfo)
        {
            if (_CurrentTrackInfo == null || _CurrentTrackInfo.track == null ||
                            _CurrentTrackInfo.track.track_resource == null ||
                            _CurrentTrackInfo.track.track_resource.uri
                            != newTrackInfo.track.track_resource.uri)
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
            return _SpotifyProcess != null;
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

        public void NextTrack() {
            if(_SpotifyProcess != null)
                PostMessage(_SpotifyProcess.MainWindowHandle, KeyMessage, IntPtr.Zero, new IntPtr(NexttrackKey));
        }

        public void PreviousTrack() {
            if (_SpotifyProcess != null)
                PostMessage(_SpotifyProcess.MainWindowHandle, KeyMessage, IntPtr.Zero, new IntPtr(PreviousKey));
        }

        public void VolumeUp() {
            if (_SpotifyProcess != null)
                PostMessage(_SpotifyProcess.MainWindowHandle, WM_COMMAND, new IntPtr(VolumeUpKey), IntPtr.Zero);
        }

        public void VolumeDown() {
            if (_SpotifyProcess != null)
                PostMessage(_SpotifyProcess.MainWindowHandle, WM_COMMAND, new IntPtr(VolumeDownKey), IntPtr.Zero);
        }

        public void OpenSpotify()
        {
            if (_SpotifyProcess != null)
            {
                WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                IntPtr handle = _SpotifyProcess.MainWindowHandle;
                GetWindowPlacement(handle, ref placement);
                if (placement.showCmd == MINIMIZED_STATE)
                {
                    ShowWindowAsync(handle, SW_RESTORE);
                    
                }
                SetForegroundWindow(handle);
            }
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

        public async Task UpdateToken(string token)
        {
            await _spotifyWebApi.UpdateToken(token);
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
