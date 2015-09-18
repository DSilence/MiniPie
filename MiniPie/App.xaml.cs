using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using MahApps.Metro;
using MiniPie.Core;
using ILog = Caliburn.Micro.ILog;

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
            ThemeManager.ChangeAppStyle(this,
                                        ThemeManager.GetAccent("Green"),
                                        ThemeManager.GetAppTheme("BaseDark"));
            base.OnStartup(e);
            await Bootstrapper.ConfigurationInitialize();
        }
    }
}
