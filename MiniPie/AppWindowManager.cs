using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Caliburn.Micro;
using MiniPie.Core;
using MiniPie.Core.Extensions;
using MiniPie.ViewModels;

namespace MiniPie {
    public sealed class AppWindowManager : WindowManager {

        [StructLayout(LayoutKind.Sequential)]
        public struct Margins {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }
        [DllImport("dwmapi.dll", PreserveSig = true)]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        [DllImport("dwmapi.dll")]
        static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

        private readonly AppSettings _Settings;

        public AppWindowManager(AppSettings settings) {
            _Settings = settings;
        }

        protected override Window CreateWindow(object rootModel, bool isDialog, object context, IDictionary<string, object> settings) {
            var wnd = base.CreateWindow(rootModel, isDialog, context, settings);
            wnd.Topmost = _Settings.AlwaysOnTop;
            wnd.SizeToContent = SizeToContent.WidthAndHeight;
            wnd.ResizeMode = ResizeMode.NoResize;
            wnd.Icon = Helper.GetImageSourceFromResource("App.ico");
            TrackLocation(wnd, rootModel);
            if(rootModel is ShellViewModel)
                SetupShell(wnd);

            var canToggleVisibility = (rootModel as IToggleVisibility);
            if (canToggleVisibility != null)
                canToggleVisibility.ToggleVisibility += (o, e) => wnd.Visibility = e.Visibility;                           

            return wnd;
        }

        private void SetupShell(Window window) {
            //Source: http://code-inside.de/blog/2012/11/11/howto-rahmenlose-wpf-apps-mit-schattenwurf/
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;
            window.ShowInTaskbar = false;
            window.MouseLeftButtonDown += (o, e) => window.DragMove();
        }

        private void TrackLocation(Window wnd, object rootViewModel) {
            var wndId = rootViewModel.GetType().Name.ToSHA1();

            var hasFixedPosition = (rootViewModel as IFixedPosition);
            if (hasFixedPosition != null) {
                wnd.WindowStartupLocation = hasFixedPosition.WindowStartupLocation;
                return;
            }

            var savedPosition = _Settings.Positions.FirstOrDefault(p => p.WindowId == wndId);
            if (savedPosition != null) {
                wnd.WindowStartupLocation = WindowStartupLocation.Manual;
                wnd.Top = savedPosition.Top;
                wnd.Left = savedPosition.Left;
            }
            else
                wnd.WindowStartupLocation = wnd.Owner != null
                                                ? WindowStartupLocation.CenterOwner
                                                : WindowStartupLocation.CenterScreen;

            if (savedPosition == null) {
                savedPosition = new WindowPosition {WindowId = wndId};
                _Settings.Positions.Add(savedPosition);
            }

            //Track the location of the Shell any time its changed
            if (rootViewModel is ShellViewModel)
                wnd.LocationChanged += (o, e) => {
                                           savedPosition.Top = ((Window) o).Top;
                                           savedPosition.Left = ((Window) o).Left;
                                       };

            wnd.Closing += (o, e) => {
                               savedPosition.Top = ((Window) o).Top;
                               savedPosition.Left = ((Window) o).Left;
                           };
        }
    }
}
