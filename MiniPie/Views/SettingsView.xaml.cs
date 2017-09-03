using System.Windows.Controls;
using MiniPie.ViewModels;

namespace MiniPie.Views {
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl {
        public SettingsView() {
            InitializeComponent();
        }

        
        private SettingsViewModel ViewModel
        {
            get { return DataContext as SettingsViewModel; }
        }
    }
}
