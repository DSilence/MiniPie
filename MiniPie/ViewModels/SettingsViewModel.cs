using System;
using System.Threading;
using Caliburn.Micro;
using MiniPie.Core;
using MiniPie.Core.Enums;
using ILog = MiniPie.Core.ILog;

namespace MiniPie.ViewModels {
	public sealed class SettingsViewModel : Screen {
		private readonly AppSettings _Settings;
		private readonly AppContracts _Contracts;
		private readonly ICoverService _CoverService;
		private readonly ILog _Logger;
	    private readonly HotKeyViewModel _hotKeyViewModel;

		public SettingsViewModel(AppSettings settings, AppContracts contracts, 
            ICoverService coverService, ILog logger, HotKeyViewModel hotKeyViewModel) {
			_Settings = settings;
			_Contracts = contracts;
			_CoverService = coverService;
			_Logger = logger;
		    _hotKeyViewModel = hotKeyViewModel;
		    DisplayName = string.Format("Settings - {0}", _Contracts.ApplicationName);
			CacheSize = Helper.MakeNiceSize(_CoverService.CacheSize());
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
                }
                NotifyOfPropertyChange();
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
	}
}