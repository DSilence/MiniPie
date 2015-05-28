using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using MiniPie.ViewModels;

namespace MiniPie.Views {
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : UserControl {
        public ShellView() {
            InitializeComponent();
        }

        private void ShellView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            var notifyIcon = this.Resources["NotifyIcon"] as TaskbarIcon;
            if (notifyIcon != null)
            {
                notifyIcon.Dispose();
            }
        }

        private void AlbumArt_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                var viewModel = ShellViewModel;
                if (viewModel != null)
                {
                    viewModel.OpenSpotifyWindow();
                }
            }
        }

        private ShellViewModel ShellViewModel
        {
            get
            {
                return DataContext as ShellViewModel;
            }
        }
    }
}
