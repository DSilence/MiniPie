using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Infralution.Localization.Wpf;
using MiniPie.Core;
using MiniPie.Core.Enums;
using ILog = MiniPie.Core.ILog;
using MiniPie.Core.SpotifyNative;

namespace MiniPie.ViewModels {
    public sealed class SettingsViewModel : Screen {
        private readonly AppSettings _settings;
        private readonly AppContracts _contracts;
        private readonly ICoverService _coverService;
        private readonly ISpotifyController _spotifyController;
        private readonly ILog _logger;
        private readonly AutorunService _autorunService;
        private readonly JsonPersister<AppSettings> _persister;

        private IDisposable _tokenUpdateSubscription;

        public SettingsViewModel(AppContracts contracts, 
            ICoverService coverService, ILog logger, HotKeyViewModel hotKeyViewModel, 
            ISpotifyController spotifyController, AutorunService autorunService, JsonPersister<AppSettings> persister) 
        {
            _settings = persister.Instance;
            _contracts = contracts;
            _coverService = coverService;
            _logger = logger;
            _spotifyController = spotifyController;
            _autorunService = autorunService;
            _persister = persister;

            HotKeyViewModel = hotKeyViewModel;
            DisplayName = $"Settings - {_contracts.ApplicationName}";
            CacheSize = Helper.MakeNiceSize(_coverService.CacheSize());
            
        }

        public bool AlwaysOnTop {
            get => _settings.AlwaysOnTop;
            set
            {
                _settings.AlwaysOnTop = value; 
                //TODO move this code somewhere
                Application.Current.MainWindow.Topmost = true;
            }
        }

        public bool StartWithWindows {
            get => _settings.StartWithWindows;
            set { _settings.StartWithWindows = value; _autorunService.ValidateAutorun();}
        }

        public bool HideIfSpotifyClosed
        {
            get => _settings.HideIfSpotifyClosed;
            set => _settings.HideIfSpotifyClosed = value;
        }

        public bool DisableAnimations {
            get => _settings.DisableAnimations;
            set => _settings.DisableAnimations = value;
        }

        public bool StartMinimized
        {
            get => _settings.StartMinimized;
            set => _settings.StartMinimized = value;
        }

        public UpdatePreference UpdatePreference
        {
            get => _settings.UpdatePreference;
            set => _settings.UpdatePreference = value;
        }

        public bool SingleClickHide
        {
            get => _settings.SingleClickHide;
            set => _settings.SingleClickHide = value;
        }

        public Language Language
        {
            get => _settings.Language;
            set
            {
                _settings.Language = value;
                if (value != null)
                {
                    Thread.CurrentThread.CurrentCulture = value.CultureInfo;
                    Thread.CurrentThread.CurrentUICulture = value.CultureInfo;
                    ResxExtension.UpdateAllTargets();
                }
            }
        }

        public LockScreenBehavior LockScreenBehavior
        {
            get => _settings.LockScreenBehavior;
            set => _settings.LockScreenBehavior = value;
        }

        public ObservableCollection<Language> Languages 
            => new ObservableCollection<Language>(LanguageHelper.Languages);

        public bool CanClearCache { get; set; } = true;

        public string CacheSize { get; set; }

        public ApplicationSize ApplicationSize {
            get => _settings.ApplicationSize;
            set => _settings.ApplicationSize = value;
        }

        public async Task ClearCache() {
            CanClearCache = false;
            try
            {
                await Task.Run(() =>_coverService.ClearCache());
                CacheSize = Helper.MakeNiceSize(_coverService.CacheSize());
            }
            catch (Exception exc)
            {
                _logger.WarnException("Failed to clear cover cache", exc);
            }
            finally
            {
                CanClearCache = true;
            }
        }

        public HotKeyViewModel HotKeyViewModel { get; }

        public bool LoginChecking { get; set; }


        public async Task UpdateLoggedIn()
        {
            LoginChecking = true;
            LoggedIn = await _spotifyController.IsUserLoggedIn();
            LoginChecking = false;
        }

        public async Task PerformLoginLogout()
        {
            if (!LoginChecking)
            {
                if (LoggedIn)
                {
                    await Logout();
                }
                else
                {
                    Login();
                }
            }
        }

        public void Login()
        {
            Process.Start(BuildLoginQuery().ToString());
        }

        public async Task Logout()
        {
            _settings.SpotifyToken = null;
            _spotifyController.Logout();
            await UpdateLoggedIn();
        }

        public string LoginStatus
        {
            get
            {
                if (LoginChecking)
                {
                    return Properties.Resources.Settings_RetrievingLoginStatus;
                }
                if (LoggedIn)
                {
                    return Properties.Resources.Settings_LoggedIn;
                }
                return Properties.Resources.Settings_NotLoggedIn;
            }
        }

        public bool LoggedIn { get; set; }


        public Uri BuildLoginQuery()
        {
            return _spotifyController.BuildLoginQuery();
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        protected override async void OnActivate()

        {
            base.OnActivate();
            HotKeyViewModel.UnregisterHotKeys();
            _tokenUpdateSubscription = _spotifyController.TokenUpdated.Subscribe(SpotifyControllerOnTokenUpdated);
            await UpdateLoggedIn();
            //TODO custom decorator would be ideal here
            PropertyChanged += SettingsViewModel_PropertyChanged;
        }

        private async void SettingsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await Task.Run(() =>
            {
                _persister.Persist();
            });
        }

        private async void SpotifyControllerOnTokenUpdated(object state)
        {
            await UpdateLoggedIn();
        }

#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            HotKeyViewModel.RegisterHotKeys();
            _tokenUpdateSubscription?.Dispose();
            PropertyChanged -= SettingsViewModel_PropertyChanged;
        }
    }
}