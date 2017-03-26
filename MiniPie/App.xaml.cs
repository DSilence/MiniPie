using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Chromium;
using MahApps.Metro;
using MiniPie.Core;
using Neutronium.Core.JavascriptFramework;
using Neutronium.JavascriptFramework.Vue;
using Neutronium.WebBrowserEngine.ChromiumFx;
using Neutronium.WPF;
using NLog.Internal;
using ILog = Caliburn.Micro.ILog;

namespace MiniPie {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : ChromiumFxWebBrowserApp
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
            /*ThemeManager.ChangeAppStyle(this,
                                        ThemeManager.GetAccent("Green"),
                                        ThemeManager.GetAppTheme("BaseDark"));*/
            base.OnStartup(e);
            await Bootstrapper.ConfigurationInitialize();
        }

        protected override void UpdateChromiumSettings(CfxSettings settings)
        {
            base.UpdateChromiumSettings(settings);
        }

        protected override void OnStartUp(IHTMLEngineFactory factory)
        {
            factory.RegisterJavaScriptFrameworkAsDefault(new VueSessionInjectorV2());
            base.OnStartUp(factory);
        }

        protected override IJavascriptFrameworkManager GetJavascriptUIFrameworkManager()
        {
            return Bootstrapper.Kernel.GetInstance<IJavascriptFrameworkManager>();
        }
    }
}
