using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;

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
    }
}
