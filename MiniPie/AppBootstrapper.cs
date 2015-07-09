using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Caliburn.Micro;
using MiniPie.Core;
using MiniPie.Core.HotKeyManager;
using MiniPie.Core.SpotifyLocal;
using MiniPie.ViewModels;
using ILog = MiniPie.Core.ILog;

namespace MiniPie {
    public sealed class AppBootstrapper : TinyBootstrapper<ShellViewModel> {

        private AppSettings _Settings;
        private AppContracts _Contracts;
        private JsonPersister<AppSettings> _SettingsPersistor;

        public AppBootstrapper()
        {
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e) {
            base.OnStartup(sender, e);

            //TODO: Find a better way
            if(Process.GetProcessesByName("MiniPie").Length > 1)
                Application.Shutdown();

        }

        protected override void Configure() {
            base.Configure();

            _Contracts = new AppContracts();
            
            _SettingsPersistor = new JsonPersister<AppSettings>(Path.Combine(_Contracts.SettingsLocation, _Contracts.SettingsFilename));
            _Settings = _SettingsPersistor.Instance;
            if (_Settings.Language != null)
            {
                Thread.CurrentThread.CurrentCulture = _Settings.Language.CultureInfo;
                Thread.CurrentThread.CurrentUICulture = _Settings.Language.CultureInfo;
            }

            Container.Register<AppContracts>(_Contracts);
            Container.Register<AppSettings>(_Settings);
            Container.Register<ILog>(new ProductionLogger());
            Container.Register<AutorunService>(new AutorunService(Container.Resolve<ILog>(), _Settings, _Contracts));
            Container.Register<IWindowManager>(new AppWindowManager(_Settings));

            Container.Register<SpotifyLocalApi>(new SpotifyLocalApi(Container.Resolve<ILog>(), _Contracts, _Settings));
            Container.Register<ISpotifyController>(new SpotifyController(Container.Resolve<ILog>(), Container.Resolve<SpotifyLocalApi>()));
            Container.Register<ICoverService>(
                new CoverService(
                    string.IsNullOrEmpty(_Settings.CacheFolder)
                        ? Directory.GetCurrentDirectory()
                        : _Settings.CacheFolder, Container.Resolve<ILog>(), Container.Resolve<SpotifyLocalApi>()));
            
            //Container.Register<IUpdateService>(new UpdateService(Container.Resolve<ILog>()));
            var keyManager = new KeyManager(Container.Resolve<ISpotifyController>());
            Container.Register<KeyManager>(keyManager);
            if (_Settings.HotKeysEnabled && _Settings.HotKeys != null)
            {
                keyManager.RegisterHotKeys(_Settings.HotKeys);
            }
        }

        protected override void OnExit(object sender, EventArgs e) {
            base.OnExit(sender, e);
            _SettingsPersistor.Dispose();
            foreach (var keyManager in Container.ResolveAll<KeyManager>())
            {
                keyManager.Dispose();
            }
        }
    }
}