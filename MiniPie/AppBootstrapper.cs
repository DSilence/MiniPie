using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Caliburn.Micro;
using MiniPie.Core;
using MiniPie.Core.SpotifyLocal;
using MiniPie.Core.SpotifyNative;
using MiniPie.Core.SpotifyNative.HotKeyManager;
using MiniPie.Core.SpotifyWeb;
using MiniPie.Manager;
using MiniPie.ViewModels;
using MiniPie.Views;
using SimpleInjector;
using ILog = MiniPie.Core.ILog;

namespace MiniPie {
    public sealed class AppBootstrapper : BootstrapperBase, IDisposable
    {

        private AppSettings _Settings;
        private AppContracts _Contracts;
        private JsonPersister<AppSettings> _SettingsPersistor;
        private ILog _log;
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
            //Not using property injection
            //var registration = _kernel.GetRegistration(instance.GetType(), true);
            //registration.Registration.InitializeInstance(instance);
        }

        public async void ProcessTokenUpdate(string input)
        {
            await _kernel.GetInstance<ISpotifyWebApi>().CreateToken(input);
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
            _kernel.RegisterSingleton<UpdateManager>();

            _kernel.RegisterSingleton(new AutorunService(_log, _Settings, _Contracts));
            _kernel.RegisterSingleton<IWindowManager>(new AppWindowManager(_Settings));
            _kernel.RegisterSingleton<ClipboardManager>();
            _kernel.Register<IEventAggregator, EventAggregator>();

            _kernel.RegisterSingleton<ISpotifyLocalApi, SpotifyLocalApi>();
            _kernel.RegisterSingleton<ISpotifyWebApi, SpotifyWebApi>();
            _kernel.RegisterSingleton<ISpotifyNativeApi, SpotifyNativeApi>();
            _kernel.RegisterSingleton<ISpotifyController, SpotifyController>();
            //new CoverService(
            _kernel.RegisterSingleton<ICoverService>(() => new CoverService(string.IsNullOrEmpty(_Settings.CacheFolder)
                ? _Contracts.SettingsLocation
                : _Settings.CacheFolder, _log, _kernel.GetInstance<ISpotifyWebApi>()));
            
            _kernel.RegisterSingleton<KeyManager>();

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
            if (_Settings.HotKeysEnabled && _Settings.HotKeys != null)
            {
                _kernel.GetInstance<KeyManager>().RegisterHotKeys(_Settings.HotKeys);
            }
            await _kernel.GetInstance<ISpotifyWebApi>().Initialize().ConfigureAwait(false);
            await _kernel.GetInstance<ISpotifyLocalApi>().Initialize().ConfigureAwait(false);
            await _kernel.GetInstance<ISpotifyController>().Initialize().ConfigureAwait(false);
            _kernel.GetInstance<UpdateManager>().Initialize();
            _kernel.GetInstance<AutorunService>().ValidateAutorun();
        }

        protected override void OnExit(object sender, EventArgs e) {
            base.OnExit(sender, e);
            Dispose();
        }

        public void Dispose()
        {
            _SettingsPersistor.Dispose();
            _kernel.Dispose();
        }
    }
}