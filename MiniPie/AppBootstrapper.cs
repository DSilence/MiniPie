using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
using MiniPie.Core;
using MiniPie.Core.HotKeyManager;
using MiniPie.Core.SpotifyLocal;
using MiniPie.Core.SpotifyWeb;
using MiniPie.ViewModels;
using Action = System.Action;
using ILog = MiniPie.Core.ILog;

namespace MiniPie {
    public sealed class AppBootstrapper : TinyBootstrapper<ShellViewModel> {

        private AppSettings _Settings;
        private AppContracts _Contracts;
        private JsonPersister<AppSettings> _SettingsPersistor;
        private ILog _log;
        private bool _secondInstance;
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
            if(Process.GetProcessesByName("MiniPie").Length > 1)
            {
                //signal existing app via named pipes
                _secondInstance = true;
            }
            else
            {
                _log.Info("First Application");
                UriProtocolManager.RegisterUrlProtocol();
                var namedPipeString = new NamedPipe<string>(NamedPipe<string>.NameTypes.PipeType1);
                namedPipeString.OnRequest += async s =>
                {
                    await Container.Resolve<SpotifyWebApi>().CreateToken(s);
                };
                namedPipeString.Start();
                Container.Register(namedPipeString);
            }
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
            Container.Register(_spotifyLocalApi);
            _spotifyWebApi = new SpotifyWebApi(Container.Resolve<ILog>(), Container.Resolve<AppSettings>());
            Container.Register(_spotifyWebApi);
            _spotifyController = new SpotifyController(Container.Resolve<ILog>(),
                Container.Resolve<SpotifyLocalApi>(), Container.Resolve<SpotifyWebApi>());
            Container.Register<ISpotifyController>(_spotifyController);
            Container.Register<ICoverService>(
                new CoverService(
                    string.IsNullOrEmpty(_Settings.CacheFolder)
                        ? Directory.GetCurrentDirectory()
                        : _Settings.CacheFolder, Container.Resolve<ILog>(), Container.Resolve<SpotifyWebApi>()));

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
            await _spotifyController.Initialize();
            await _spotifyLocalApi.Initialize();

        }

        protected override void OnExit(object sender, EventArgs e) {
            base.OnExit(sender, e);
            if(!_secondInstance)
                UriProtocolManager.UnregisterUrlProtocol();
            _SettingsPersistor.Dispose();
            foreach (var keyManager in Container.ResolveAll<KeyManager>())
            {
                keyManager.Dispose();
            }
        }
    }
}