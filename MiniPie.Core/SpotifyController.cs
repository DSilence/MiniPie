using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using MiniPie.Core.SpotifyLocal;
using Message = System.Windows.Forms.Message;
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

        #region Win32Imports

        private const int SW_RESTORE = 9;
        private const int MINIMIZED_STATE = 2;
        

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32")]
        private static extern bool GetMessage(ref Message lpMsg, IntPtr handle, uint mMsgFilterInMain, uint mMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

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

        private const long PlaypauseKey = 0xE0000L;
        private const long NexttrackKey = 0xB0000L;
        private const long PreviousKey = 0xC0000L;
        private const long VolumeUpKey = 0x10079L;
        private const long VolumeDownKey = 0x1007AL;

        private const string SpotifyRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\Spotify";

        private readonly ILog _Logger;
        private readonly SpotifyLocalApi _LocalApi;

        private Process _SpotifyProcess;
        private Thread _BackgroundChangeTracker;
        private Timer _ProcessWatcher;
        private Timer _songStatusWatcher;
        private Status _CurrentTrackInfo;
        private WinEventDelegate _ProcDelegate;
        private object _syncObject = new object();

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        public SpotifyController(ILog logger, SpotifyLocalApi localApi) {
            _Logger = logger;
            _LocalApi = localApi;
            AttachToProcess();
            JoinBackgroundProcess();
            _songStatusWatcher = new Timer(SongTimerChanging, null, 1000, 1000);

            if(_SpotifyProcess == null)
                WaitForSpotify();
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

        private void JoinBackgroundProcess() {
            if (_BackgroundChangeTracker != null && _BackgroundChangeTracker.IsAlive)
                return;
            
            _BackgroundChangeTracker = new Thread(BackgroundChangeTrackerWork) { IsBackground = true };
            _BackgroundChangeTracker.Start();
        }

        private void AttachToProcess() {
            _SpotifyProcess = null;
            _SpotifyProcess = Process.GetProcessesByName("spotify")
                .FirstOrDefault(p => p.MainWindowHandle.ToInt32() > 0);
            lock (_syncObject)
            {
                if (_SpotifyProcess != null)
                {
                    //Renew token for Spotify local api
                    _LocalApi.RenewToken();

                    _SpotifyProcess.EnableRaisingEvents = true;
                    _SpotifyProcess.Exited += (o, e) =>
                    {
                        _SpotifyProcess = null;
                        _BackgroundChangeTracker.Abort();
                        _BackgroundChangeTracker = null;
                        WaitForSpotify();
                        OnSpotifyExited();
                    };
                }
            }
        }

        private void WaitForSpotify() {
            _ProcessWatcher = new Timer(WaitForSpotifyCallback, null, 1000, 1000);
        }

        private void WaitForSpotifyCallback(object args) {
            AttachToProcess();
            if (_SpotifyProcess != null) {
             
                //Start track change tracker
                JoinBackgroundProcess();

                //Kill timer
                if (_ProcessWatcher != null) {
                    _ProcessWatcher.Dispose();
                    _ProcessWatcher = null;
                }

                //Notify UI that Spotify is available
                OnSpotifyOpenend();
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
            try {
                if (_SpotifyProcess == null) //Spotify is not running :-(
                return;
                //get immediate status as soon as possible
                _CurrentTrackInfo = await _LocalApi.SendLocalStatusRequest(true, true, -1);
                OnTrackChanged();
                _songStatusWatcher.Change(
                                GetDelayForPlaybackUpdate(_CurrentTrackInfo.playing_position), 1000);

                while (true)
                {
                    if (_SpotifyProcess != null)
                    {
                        var newTrackInfo = await _LocalApi.SendLocalStatusRequest(true, true, 60);
                        if (newTrackInfo == null)
                        {
                            //TODO not sure when it happens, should probably happen never
                            throw new ApplicationException("Unable to retrieve track info");
                        }
                        try
                        {
                            if (newTrackInfo.error != null)
                            {
                                if ((newTrackInfo.error.message.Contains("Invalid Csrf token") ||
                                     newTrackInfo.error.message.Contains("Expired OAuth token")))
                                {
                                    //try to renew token and retrieve status again
                                    _LocalApi.RenewToken();
                                    newTrackInfo = await _LocalApi.SendLocalStatusRequest(true, true, 60);
                                    if (newTrackInfo == null)
                                    {
                                        throw new ApplicationException("Unable to retrieve track info");
                                    }
                                    if (newTrackInfo.error != null)
                                    {
                                        throw new Exception(string.Format("Spotify API error: {0}",
                                            newTrackInfo.error.message));
                                    }
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
                            _Logger.WarnException("Failed to retrieve trackinfo", exc);
                            _CurrentTrackInfo = null;
                            continue;
                        }

                        
                        if (_CurrentTrackInfo == null ||
                            _CurrentTrackInfo.track.track_resource.uri
                            != newTrackInfo.track.track_resource.uri)
                        {
                            _CurrentTrackInfo = newTrackInfo;
                            OnTrackChanged();
                            //need to increase the playback timer every second
                            //the initial delay depends on 
                            _songStatusWatcher.Change(
                                GetDelayForPlaybackUpdate(newTrackInfo.playing_position), 1000);
                        }
                        else
                        {
                            _CurrentTrackInfo = newTrackInfo;
                            OnTrackTimerChanged();
                            _songStatusWatcher.Change(
                                GetDelayForPlaybackUpdate(newTrackInfo.playing_position), 1000);
                        }
                    }
                }
            }
            catch (ThreadAbortException) { /* Thread was aborted, accept it */ }
            catch (Exception exc) {
                _Logger.WarnException("BackgroundChangeTrackerWork failed", exc);
                Console.WriteLine(exc.ToString());
            }
        }

        private int GetDelayForPlaybackUpdate(double playPosition)
        {
            int i = (int) playPosition;
            double fract = playPosition - i;
            int result =  (int) (1000 - fract*1000);
            return result;
        }

        private string GetSpotifyWindowTitle() {
            if(_SpotifyProcess == null)
                return string.Empty;

            // Allocate correct string length first
            var length = GetWindowTextLength(_SpotifyProcess.MainWindowHandle);
            var sb = new StringBuilder(length + 1);
            GetWindowText(_SpotifyProcess.MainWindowHandle, sb, sb.Capacity);
            return sb.ToString();
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
                //In case of an error it's better to return true instead of false, because this makes Winfy unusable if there is something wrong with Windows.
                return true;
            }
        }

        public string GetSongName() {
            if (_CurrentTrackInfo != null && _CurrentTrackInfo.track != null && _CurrentTrackInfo.track.track_resource != null)
                return _CurrentTrackInfo.track.track_resource.name;

            var title = GetSpotifyWindowTitle().Split('–');
            return title.Count() > 1 ? title[1].Trim() : string.Empty;
        }

        public string GetArtistName() {
            if (_CurrentTrackInfo != null && _CurrentTrackInfo.track != null && _CurrentTrackInfo.track.artist_resource != null)
                return _CurrentTrackInfo.track.artist_resource.name;

            var title = GetSpotifyWindowTitle().Split('–');
            return title.Count() > 1 ? title[0].Split('-')[1].Trim() : string.Empty;
        }

        public Status GetStatus() {
            return _CurrentTrackInfo;
        }

        public void PausePlay() {
            PostMessage(_SpotifyProcess.MainWindowHandle, KeyMessage, IntPtr.Zero, new IntPtr(PlaypauseKey));
        }

        public void NextTrack() {
            PostMessage(_SpotifyProcess.MainWindowHandle, KeyMessage, IntPtr.Zero, new IntPtr(NexttrackKey));
        }

        public void PreviousTrack() {
            PostMessage(_SpotifyProcess.MainWindowHandle, KeyMessage, IntPtr.Zero, new IntPtr(PreviousKey));
        }

        public void VolumeUp() {
            PostMessage(_SpotifyProcess.MainWindowHandle, WM_COMMAND, new IntPtr(VolumeUpKey), IntPtr.Zero);
        }

        public void VolumeDown() {
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
                else
                {
                    SetForegroundWindow(handle);
                }
            }
        }

        public void Dispose()
        {
            _songStatusWatcher.Dispose();
            if(_BackgroundChangeTracker.IsAlive)
                _BackgroundChangeTracker.Abort();
        }
    }
}
