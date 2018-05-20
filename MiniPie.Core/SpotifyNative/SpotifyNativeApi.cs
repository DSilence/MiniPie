using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MiniPie.Core.SpotifyNative
{
#pragma warning disable CA1060 // Move P/Invokes to native methods class
    public class SpotifyNativeApi: ISpotifyNativeApi, IDisposable
#pragma warning restore CA1060 // Move P/Invokes to native methods class
    {
        public Process SpotifyProcess { get; set; }

        #region Win32Imports

        private const int KeyMessage = 0x319;

        private const long NexttrackKey = 0xB0000L;
        private const long PreviousKey = 0xC0000L;
        private const int SW_RESTORE = 9;
        private const int MINIMIZED_STATE = 2;


        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Point rcNormalPosition;
        }
        #endregion

        public async Task AttachToProcess(Func<Process, Task> processFound, Action<Process> processExited)
        {
            SpotifyProcess = null;
            SpotifyProcess = Process.GetProcessesByName("spotify")
                .FirstOrDefault(p => !string.IsNullOrEmpty(p.MainWindowTitle));
            if (SpotifyProcess != null)
            {
                await processFound(SpotifyProcess).ConfigureAwait(false);
                //Renew updateToken for Spotify local api
                
                SpotifyProcess.EnableRaisingEvents = true;
                SpotifyProcess.Exited += (o, e) =>
                {
                    processExited(SpotifyProcess);
                    SpotifyProcess = null;
                    
                };
            }
        }

        public void NextTrack()
        {
            if (SpotifyProcess != null)
                PostMessage(SpotifyProcess.MainWindowHandle, KeyMessage, IntPtr.Zero, new IntPtr(NexttrackKey));
        }

        public void PreviousTrack()
        {
            if (SpotifyProcess != null)
                PostMessage(SpotifyProcess.MainWindowHandle, KeyMessage, IntPtr.Zero, new IntPtr(PreviousKey));
        }

        public void OpenSpotify()
        {
            if (SpotifyProcess != null)
            {
                WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                IntPtr handle = SpotifyProcess.MainWindowHandle;
                GetWindowPlacement(handle, ref placement);
                if (placement.showCmd == MINIMIZED_STATE)
                {
                    ShowWindowAsync(handle, SW_RESTORE);

                }
                SetForegroundWindow(handle);
            }
        }

        public void Dispose()
        {
            SpotifyProcess?.Dispose();
        }
    }
}