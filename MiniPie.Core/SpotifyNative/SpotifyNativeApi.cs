using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MiniPie.Core.SpotifyNative
{
    public class SpotifyNativeApi: ISpotifyNativeApi
    {
        public Process SpotifyProcess { get; set; }

        #region Win32Imports

        private const int KeyMessage = 0x319;
        private const uint WM_COMMAND = 0x0111;

        private const long NexttrackKey = 0xB0000L;
        private const long PreviousKey = 0xC0000L;
        private const long VolumeUpKey = 0x10079L;
        private const long VolumeDownKey = 0x1007AL;
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

        public void VolumeUp()
        {
            if (SpotifyProcess != null)
                PostMessage(SpotifyProcess.MainWindowHandle, WM_COMMAND, new IntPtr(VolumeUpKey), IntPtr.Zero);
        }

        public void VolumeDown()
        {
            if (SpotifyProcess != null)
                PostMessage(SpotifyProcess.MainWindowHandle, WM_COMMAND, new IntPtr(VolumeDownKey), IntPtr.Zero);
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
    }
}