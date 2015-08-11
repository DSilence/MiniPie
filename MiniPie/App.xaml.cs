using System;
using System.Diagnostics;
using System.Linq;
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
                if (Process.GetProcessesByName("MiniPie").Length > 1)
                {
                    string code = e.Args.FirstOrDefault();
                    if (code != null)
                    {
                        NamedPipe<string>.Send(NamedPipe<string>.NameTypes.PipeType1, code);
                    }
                    Shutdown();
                }
            AppBootstrapper bootstrapper = (AppBootstrapper)Resources["bootstrapper"];
            await bootstrapper.ConfigurationInitialize();
            
        }
    }
}
