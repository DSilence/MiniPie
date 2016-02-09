using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Caliburn.Micro;
using MiniPie.Core;
using MiniPie.Core.HotKeyManager;
using MiniPie.Core.SpotifyLocal;
using MiniPie.Core.SpotifyWeb;
using MiniPie.ViewModels;
using MiniPie.Views;
using SimpleInjector;
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
        private readonly Container _kernel = new Container();

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
            return _kernel.GetInstance(service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return new List<object>
            {
                _kernel.GetInstance(service)
            };
            //No real need to resolve all
            /*
            IServiceProvider provider = _kernel;
            Type collectionType = typeof(IEnumerable<>).MakeGenericType(service);
            var services = (IEnumerable<object>)provider.GetService(collectionType);
            return services ?? Enumerable.Empty<object>();*/
        }

        protected override void BuildUp(object instance)
        {
            var registration = _kernel.GetRegistration(instance.GetType(), true);
            registration.Registration.InitializeInstance(instance);
        }

        public async void ProcessTokenUpdate(string input)
        {
            await _kernel.GetInstance<ISpotifyWebApi>().CreateToken(input);
            var processStartInfo = new ProcessStartInfo("MiniPieHelper.exe", "unregisterUri");
            processStartInfo.Verb = "runas";
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(processStartInfo);
        }

        protected override void Configure() {
            _Contracts = new AppContracts();
            _kernel.RegisterSingleton(this);
            _SettingsPersistor = new JsonPersister<AppSettings>(Path.Combine(_Contracts.SettingsLocation, _Contracts.SettingsFilename));
            _kernel.RegisterSingleton(_SettingsPersistor);
            _kernel.RegisterSingleton(_Contracts);
            _Settings = _SettingsPersistor.Instance;
            _kernel.RegisterSingleton(_Settings);
            
            _log = new ProductionLogger();
            _kernel.RegisterSingleton(_log);

            _kernel.RegisterSingleton(new AutorunService(_log, _Settings, _Contracts));
            _kernel.RegisterSingleton<IWindowManager>(new AppWindowManager(_Settings));
            _kernel.Register<IEventAggregator, EventAggregator>();

            _spotifyLocalApi = new SpotifyLocalApi(_log, _Contracts);
            _kernel.RegisterSingleton<ISpotifyLocalApi>(_spotifyLocalApi);
            _spotifyWebApi = new SpotifyWebApi(_log, _Settings);
            _kernel.RegisterSingleton<ISpotifyWebApi>(_spotifyWebApi);
            _spotifyController = new SpotifyController(_log,
                _spotifyLocalApi, _spotifyWebApi);
            _kernel.RegisterSingleton<ISpotifyController>(_spotifyController);
            _kernel.RegisterSingleton<ICoverService>(new CoverService(
                string.IsNullOrEmpty(_Settings.CacheFolder)
                    ? Directory.GetCurrentDirectory()
                    : _Settings.CacheFolder, _log, _spotifyWebApi));

            //Container.Register<IUpdateService>(new UpdateService(Container.Resolve<ILog>()));
            var keyManager = new KeyManager(_spotifyController, _log);
            _kernel.RegisterSingleton(keyManager);
            if (_Settings.HotKeysEnabled && _Settings.HotKeys != null)
            {
                keyManager.RegisterHotKeys(_Settings.HotKeys);
            }

            var classes =
                Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(
                        type =>
                            type.IsPublic && (
                            type.Namespace == typeof (ShellViewModel).Namespace ||
                            type.Namespace == typeof (ShellView).Namespace))
                    .ToList();
            foreach (var @class in classes)
            {
                _kernel.Register(@class);
            }
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
                
            foreach (var keyManager in _kernel.GetAllInstances<KeyManager>())
            {
                keyManager.Dispose();
            }
            _kernel.Dispose();
        }
    }
}