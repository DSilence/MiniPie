using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace MiniPie {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Bootstrapper = new AppBootstrapper();
            Bootstrapper.Initialize();
            InitializeComponent();
        }

        public AppBootstrapper Bootstrapper { get; set; }
        protected override async void OnStartup(StartupEventArgs e)
        {
            await Bootstrapper.ConfigurationInitialize();
            base.OnStartup(e);
        }
    }
}
