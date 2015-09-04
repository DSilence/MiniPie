using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly HotKeyViewModel _hotKeyViewModel;

        public SettingsViewModel(AppSettings settings, AppContracts contracts, 
            ICoverService coverService, ILog logger, HotKeyViewModel hotKeyViewModel, 
            ISpotifyController spotifyController) 
        {
            _Settings = settings;
            _Contracts = contracts;
            _CoverService = coverService;
            _Logger = logger;
            _hotKeyViewModel = hotKeyViewModel;
            _spotifyController = spotifyController;
            DisplayName = string.Format("Settings - {0}", _Contracts.ApplicationName);
            CacheSize = Helper.MakeNiceSize(_CoverService.CacheSize());
            UpdateLoggedIn();
        }

        public bool AlwaysOnTop {
            get { return _Settings.AlwaysOnTop; }
            set { _Settings.AlwaysOnTop = value; NotifyOfPropertyChange(); }
        }

        public bool StartWithWindows {
            get { return _Settings.StartWithWindows; }
            set { _Settings.StartWithWindows = value; NotifyOfPropertyChange(); }
        }

        public bool HideIfSpotifyClosed {
            get { return _Settings.HideIfSpotifyClosed; }
            set { _Settings.HideIfSpotifyClosed = value; NotifyOfPropertyChange(); }
        }

        public bool DisableAnimations {
            get { return _Settings.DisableAnimations; }
            set { _Settings.DisableAnimations = value; NotifyOfPropertyChange(); }
        }

        public bool StartMinimized
        {
            get { return _Settings.StartMinimized; }
            set { _Settings.StartMinimized = value; NotifyOfPropertyChange(); }
        }

        public Language Language
        {
            get { return _Settings.Language ?? (_Settings.Language = LanguageHelper.English); }
            set
            {
                _Settings.Language = value;
                if (value != null)
                {
                    Thread.CurrentThread.CurrentCulture = value.CultureInfo;
                    Thread.CurrentThread.CurrentUICulture = value.CultureInfo;
                    ResxExtension.UpdateAllTargets();
                }
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<Language> Languages
        {
            get
            {
                return new ObservableCollection<Language>(LanguageHelper.Languages);
            }
        } 

        private bool _CanClearCache = true;
        public bool CanClearCache {
            get { return _CanClearCache; }
            set { _CanClearCache = value; NotifyOfPropertyChange(); }
        }

        private string _CacheSize;
        public string CacheSize {
            get { return _CacheSize; }
            set { _CacheSize = value; NotifyOfPropertyChange(); }
        }

        public ApplicationSize ApplicationSize {
            get { return _Settings.ApplicationSize; }			
            set { _Settings.ApplicationSize = value; NotifyOfPropertyChange(); }
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

        public HotKeyViewModel HotKeyViewModel
        {
            get { return _hotKeyViewModel; }
        }

        private bool _loginChecking;

        public bool LoginChecking
        {
            get { return _loginChecking; }
            set
            {
                _loginChecking = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => LoginStatus);
            }
        }


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
            var processStartInfo = new ProcessStartInfo("MiniPieHelper.exe", "registerUri");
            processStartInfo.Verb = "runas";
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            using (var utils = new Process())
            {
                utils.StartInfo = processStartInfo;
                utils.EnableRaisingEvents = true;
                utils.Start();
                Process.Start(BuildLoginQuery().ToString());
            }
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

        private bool _loggedIn;
        public bool LoggedIn
        {
            get { return _loggedIn; }
            set { _loggedIn = value; NotifyOfPropertyChange(); }
        }


        public Uri BuildLoginQuery()
        {
            return _spotifyController.BuildLoginQuery();
        }

        public async Task UpdateToken(string token)
        {
            await _spotifyController.UpdateToken(token);
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