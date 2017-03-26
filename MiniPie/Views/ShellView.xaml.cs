using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Hardcodet.Wpf.TaskbarNotification;
using MiniPie.ViewModels;
using Application = System.Windows.Application;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using FlowDirection = System.Windows.FlowDirection;
using Size = System.Windows.Size;
using UserControl = System.Windows.Controls.UserControl;
using System.Windows.Controls.Primitives;

namespace MiniPie.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : UserControl
    {
        private readonly TaskbarIcon _notifyIcon;

        public ShellView()
        {
            InitializeComponent();
            _notifyIcon = (TaskbarIcon) this.Resources["NotifyIcon"];
        }

        private void MainWindowOnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            _notifyIcon.Visibility = Visibility.Collapsed;
            _notifyIcon.Dispose();
        }

        private void AlbumArt_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                var viewModel = ShellViewModel;
                if (viewModel != null)
                {
                    // viewModel.OpenSpotifyWindow();
                }
            }
        }

        private ShellViewModel ShellViewModel
        {
            get { return DataContext as ShellViewModel; }
        }

        private void ShellView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Application.Current.MainWindow.Closing -= MainWindowOnClosing;
            Application.Current.MainWindow.Closing += MainWindowOnClosing;
            _notifyIcon.ContextMenu.DataContext = e.NewValue;
        }


        private const int InitialBeginAnimationCyclePause = 1;
        private const int BeginAnimationCyclePause = 0;
        private const int EndAnimationCyclePause = 5;
        private double _speed;
        private readonly RepeatBehavior _once = new RepeatBehavior(1);

        private bool shouldAnimate;
        private bool _isStoryboardRunning;

        private void NotifyIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ShellViewWindow.Uri = new Uri("http://google.com");
            // ShellViewModel.HandleTrayMouseDoubleClick(Application.Current.MainWindow);
        }

        private void NotifyIcon_OnTrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            // ShellViewModel.HandleTrayMouseClick(Application.Current.MainWindow);
        }
    }
}