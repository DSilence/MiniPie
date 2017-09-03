using System.Windows;
using MahApps.Metro;

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
            await Bootstrapper.ConfigurationInitialize();
            base.OnStartup(e);
        }
    }
}
