using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using MiniPie.Core.SpotifyLocal;
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
        private Timer _songStatusWatcher;
        private Status _CurrentTrackInfo;
        private WinEventDelegate _ProcDelegate;

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        public SpotifyController(ILog logger, SpotifyLocalApi localApi) {
            _Logger = logger;
            _LocalApi = localApi;

            
            AttachToProcess().Wait();
            _songStatusWatcher = new Timer(SongTimerChanging, null, 1000, 1000);
            JoinBackgroundProcess();
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
                await _LocalApi.RenewToken();
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
                        int timeout = _CurrentTrackInfo == null ? -1 : 60;
                        Status newTrackInfo;
                        try
                        {
                            Debug.Print("Started retrieving information from spotify, timeout is" + timeout);
                            newTrackInfo =
                                await _LocalApi.SendLocalStatusRequest(true, true, timeout);
                            Debug.Print("Finished retrieving information from spotify");
                        }
                        catch (TaskCanceledException)
                        {
                            Debug.Print("Retrieving cancelled");
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
                                if ((newTrackInfo.error.message.Contains("Invalid Csrf updateToken") ||
                                     newTrackInfo.error.message.Contains("OAuth updateToken") ||
                                     newTrackInfo.error.message.Contains("Expired OAuth token")))
                                {
                                    //try to renew updateToken and retrieve status again
                                    _Logger.Info("Renew updateToken and try again");
                                    await _LocalApi.RenewToken();
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
                _Logger.WarnException("BackgroundChangeTrackerWork failed", exc);
                Console.WriteLine(exc.ToString());
            }
        }

        private void ProcessTrackInfo(Status newTrackInfo)
        {
            if (_CurrentTrackInfo == null || _CurrentTrackInfo.track == null ||
                            _CurrentTrackInfo.track.track_resource == null ||
                            _CurrentTrackInfo.track.track_resource.uri
                            != newTrackInfo.track.track_resource.uri)
            {
                _CurrentTrackInfo = newTrackInfo;
                OnTrackChanged();
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

        public void PausePlay()
        {
            if(_SpotifyProcess != null)
                PostMessage(_SpotifyProcess.MainWindowHandle, KeyMessage, IntPtr.Zero, new IntPtr(PlaypauseKey));
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
