using System;
using System.Collections.Generic;
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
using MiniPie.Views;
using Ninject;
using Ninject.Extensions.Conventions;
using ILog = MiniPie.Core.ILog;

namespace MiniPie {
    public sealed class AppBootstrapper : BootstrapperBase {

        private AppSettings _Settings;
        private AppContracts _Contracts;
        private JsonPersister<AppSettings> _SettingsPersistor;
        private ILog _log;
        private SpotifyWebApi _spotifyWebApi;
        private SpotifyController _spotifyController;
        private SpotifyLocalApi _spotifyLocalApi;
        private readonly IKernel _kernel = new StandardKernel();

        public AppBootstrapper():base(true)
        {
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            base.OnStartup(sender, e);

            _log.Info("Starting");
            DisplayRootViewFor<ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _kernel.Get(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _kernel.GetAll(service);
        }

        protected override void BuildUp(object instance)
        {
            _kernel.Inject(instance);
        }

        public async void ProcessTokenUpdate(string input)
        {
            await _kernel.Get<ISpotifyWebApi>().CreateToken(input);
            var processStartInfo = new ProcessStartInfo("MiniPieHelper.exe", "unregisterUri");
            processStartInfo.Verb = "runas";
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(processStartInfo);
        }

        protected override void Configure() {
            _Contracts = new AppContracts();

            _SettingsPersistor = new JsonPersister<AppSettings>(Path.Combine(_Contracts.SettingsLocation, _Contracts.SettingsFilename));
            _kernel.Bind<JsonPersister<AppSettings>>().ToConstant(_SettingsPersistor).InSingletonScope();
            _kernel.Bind<AppContracts>().ToConstant(_Contracts).InSingletonScope();
            _Settings = _SettingsPersistor.Instance;
            _kernel.Bind<AppSettings>().ToConstant(_Settings).InSingletonScope();
            
            _log = new ProductionLogger();
            _kernel.Bind<ILog>().ToConstant(_log).InSingletonScope();

            _kernel.Bind<AutorunService>()
                .ToConstant(new AutorunService(_log, _Settings, _Contracts))
                .InSingletonScope();
            _kernel.Bind<IWindowManager>().ToConstant(new AppWindowManager(_Settings)).InSingletonScope();
            _kernel.Bind<IEventAggregator>().To<EventAggregator>();

            _spotifyLocalApi = new SpotifyLocalApi(_log, _Contracts, _Settings);
            _kernel.Bind<ISpotifyLocalApi>().ToConstant(_spotifyLocalApi).InSingletonScope();
            _spotifyWebApi = new SpotifyWebApi(_log, _Settings);
            _kernel.Bind<ISpotifyWebApi>().ToConstant(_spotifyWebApi).InSingletonScope();
            _spotifyController = new SpotifyController(_log,
                _spotifyLocalApi, _spotifyWebApi);
            _kernel.Bind<ISpotifyController>().ToConstant(_spotifyController).InSingletonScope();
            _kernel.Bind<ICoverService>().ToConstant(new CoverService(
                string.IsNullOrEmpty(_Settings.CacheFolder)
                    ? Directory.GetCurrentDirectory()
                    : _Settings.CacheFolder, _log, _spotifyWebApi)).InSingletonScope();

            //Container.Register<IUpdateService>(new UpdateService(Container.Resolve<ILog>()));
            var keyManager = new KeyManager(_spotifyController, _log);
            _kernel.Bind<KeyManager>().ToConstant(keyManager).InSingletonScope();
            if (_Settings.HotKeysEnabled && _Settings.HotKeys != null)
            {
                keyManager.RegisterHotKeys(_Settings.HotKeys);
            }

            _kernel.Bind(
                x => x.FromThisAssembly().SelectAllClasses().InNamespaceOf(typeof (ShellViewModel), typeof(ShellView)).BindToSelf());
            base.Configure();
        }

        public async Task ConfigurationInitialize()
        {
            await _spotifyWebApi.Initialize();
            await _spotifyLocalApi.Initialize();
            await _spotifyController.Initialize();
        }

        protected override void OnExit(object sender, EventArgs e) {
            base.OnExit(sender, e);
                
            foreach (var keyManager in _kernel.GetAll<KeyManager>())
            {
                keyManager.Dispose();
            }
            _kernel.Dispose();
        }
    }
}