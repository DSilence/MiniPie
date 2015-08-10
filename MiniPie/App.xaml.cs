using System;
using System.Windows;
using System.Windows.Navigation;

namespace MiniPie {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
        }

        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            AppBootstrapper bootstrapper = (AppBootstrapper)Resources["bootstrapper"];
            await bootstrapper.ConfigurationInitialize();
        }
    }
}
