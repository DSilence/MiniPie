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

namespace MiniPie.ViewModels {
    public sealed class SettingsViewModel : Screen {
        private readonly AppSettings _Settings;
        private readonly AppContracts _Contracts;
        private readonly ICoverService _CoverService;
        private readonly ISpotifyController _spotifyController;
        private readonly ILog _Logger;
        private readonly AutorunService _autorunService;

        public SettingsViewModel(AppContracts contracts, 
            ICoverService coverService, ILog logger, HotKeyViewModel hotKeyViewModel, 
            ISpotifyController spotifyController, AutorunService autorunService, JsonPersister<AppSettings> persister) 
        {
            _Settings = persister.Instance;
            _Contracts = contracts;
            _CoverService = coverService;
            _Logger = logger;
            HotKeyViewModel = hotKeyViewModel;
            _spotifyController = spotifyController;
            _autorunService = autorunService;
            DisplayName = string.Format("Settings - {0}", _Contracts.ApplicationName);
            CacheSize = Helper.MakeNiceSize(_CoverService.CacheSize());
            UpdateLoggedIn();
            //TODO custom decorator would be ideal here
            PropertyChanged += (sender, args) =>
            {
                Task.Run(() =>
                {
                    persister.Persist();
                });
            };
        }

        public bool AlwaysOnTop {
            get { return _Settings.AlwaysOnTop; }
            set
            {
                _Settings.AlwaysOnTop = value; 
                //TODO move this code somewhere
                Application.Current.MainWindow.Topmost = true;
            }
        }

        public bool StartWithWindows {
            get { return _Settings.StartWithWindows; }
            set { _Settings.StartWithWindows = value; _autorunService.ValidateAutorun();}
        }

        public bool HideIfSpotifyClosed {
            get { return _Settings.HideIfSpotifyClosed; }
            set { _Settings.HideIfSpotifyClosed = value;}
        }

        public bool DisableAnimations {
            get { return _Settings.DisableAnimations; }
            set { _Settings.DisableAnimations = value;}
        }

        public bool StartMinimized
        {
            get { return _Settings.StartMinimized; }
            set { _Settings.StartMinimized = value;}
        }

        public Language Language
        {
            get { return _Settings.Language; }
            set
            {
                _Settings.Language = value;
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
            get { return _Settings.LockScreenBehavior; }
            set { _Settings.LockScreenBehavior = value; }
        }

        public ObservableCollection<Language> Languages 
            => new ObservableCollection<Language>(LanguageHelper.Languages);
        public ObservableCollection<LockScreenBehavior> LockScreenBehaviors
            => new ObservableCollection<LockScreenBehavior>(); 

        public bool CanClearCache { get; set; } = true;

        public string CacheSize { get; set; }

        public ApplicationSize ApplicationSize {
            get { return _Settings.ApplicationSize; }			
            set { _Settings.ApplicationSize = value;}
        }

        public void ClearCache() {
            try {
                _CoverService.ClearCache();
                CacheSize = Helper.MakeNiceSize(_CoverService.CacheSize());
            }
            catch (Exception exc) {
                _Logger.WarnException("Failed to clear cover cache", exc);
            }
            CanClearCache = false;
        }

        public HotKeyViewModel HotKeyViewModel { get; }

        public bool LoginChecking { get; set; }


        public async void UpdateLoggedIn()
        {
            LoginChecking = true;
            LoggedIn = await _spotifyController.IsUserLoggedIn();
            LoginChecking = false;
        }

        public void PerformLoginLogout()
        {
            if (!LoginChecking)
            {
                if (LoggedIn)
                {
                    Logout();
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

        public void Logout()
        {
            _Settings.SpotifyToken = null;
            _spotifyController.Logout();
            UpdateLoggedIn();
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

        protected override void OnActivate()
        {
            base.OnActivate();
            _spotifyController.TokenUpdated += SpotifyControllerOnTokenUpdated;
        }

        private void SpotifyControllerOnTokenUpdated(object sender, EventArgs eventArgs)
        {
            UpdateLoggedIn();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            _spotifyController.TokenUpdated -= SpotifyControllerOnTokenUpdated;
        }
    }
}