using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using MiniPie.Core;
using MiniPie.Core.HotKeyManager;
using MiniPie.Core.SpotifyLocal;
using MiniPie.Core.SpotifyWeb;
using MiniPie.ViewModels;
using ILog = MiniPie.Core.ILog;

namespace MiniPie {
    public sealed class AppBootstrapper : TinyBootstrapper<ShellViewModel> {

        private AppSettings _Settings;
        private AppContracts _Contracts;
        private JsonPersister<AppSettings> _SettingsPersistor;
        private ILog _log;
        private SpotifyWebApi _spotifyWebApi;
        private SpotifyController _spotifyController;
        private SpotifyLocalApi _spotifyLocalApi;

        public AppBootstrapper()
        {
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            base.OnStartup(sender, e);

            _log.Info("Starting");
        }

        public async void ProcessTokenUpdate(string input)
        {
            await Container.Resolve<ISpotifyWebApi>().CreateToken(input);
            var processStartInfo = new ProcessStartInfo("MiniPieHelper.exe", "unregisterUri");
            processStartInfo.Verb = "runas";
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(processStartInfo);
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
            _log = new ProductionLogger();
            Container.Register<ILog>(_log);
            Container.Register<AutorunService>(new AutorunService(Container.Resolve<ILog>(), _Settings, _Contracts));
            Container.Register<IWindowManager>(new AppWindowManager(_Settings));

            _spotifyLocalApi = new SpotifyLocalApi(Container.Resolve<ILog>(), _Contracts, _Settings);
            Container.Register<ISpotifyLocalApi>(_spotifyLocalApi);
            _spotifyWebApi = new SpotifyWebApi(Container.Resolve<ILog>(), Container.Resolve<AppSettings>());
            Container.Register<ISpotifyWebApi>(_spotifyWebApi);
            _spotifyController = new SpotifyController(Container.Resolve<ILog>(),
                _spotifyLocalApi, _spotifyWebApi);
            Container.Register<ISpotifyController>(_spotifyController);
            Container.Register<ICoverService>(
                new CoverService(
                    string.IsNullOrEmpty(_Settings.CacheFolder)
                        ? Directory.GetCurrentDirectory()
                        : _Settings.CacheFolder, Container.Resolve<ILog>(), _spotifyWebApi));

            //Container.Register<IUpdateService>(new UpdateService(Container.Resolve<ILog>()));
            var keyManager = new KeyManager(Container.Resolve<ISpotifyController>(), Container.Resolve<ILog>());
            Container.Register<KeyManager>(keyManager);
            if (_Settings.HotKeysEnabled && _Settings.HotKeys != null)
            {
                keyManager.RegisterHotKeys(_Settings.HotKeys);
            }
        }

        public async Task ConfigurationInitialize()
        {
            await _spotifyWebApi.Initialize();
            await _spotifyLocalApi.Initialize();
            await _spotifyController.Initialize();
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