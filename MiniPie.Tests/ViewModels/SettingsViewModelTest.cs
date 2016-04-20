using MiniPie.Core;
using MiniPie.Core.Enums;
using MiniPie.ViewModels;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MiniPie.Tests.ViewModels
{
    public class SettingsViewModelTest
    {
        private SettingsViewModel _settingsViewModel;
        private ICoverService _coverService;
        private ILog _logger;
        private HotKeyViewModel _hotkeyViewModel;
        private ISpotifyController _spotifyController;
        private AutorunService _autorunService;
        private JsonPersister<AppSettings> _appSettingsPersistor;
        private AppSettings _appSettings;

        public SettingsViewModelTest()
        {
            _coverService = Substitute.For<ICoverService>();
            _logger = Substitute.For<ILog>();
            _appSettings = Substitute.For<AppSettings>();
            _hotkeyViewModel = Substitute.For<HotKeyViewModel>(null, _appSettings);
            _spotifyController = Substitute.For<ISpotifyController>();
            _autorunService = Substitute.For<AutorunService>(null, null, null);
            _appSettingsPersistor = Substitute.For<JsonPersister<AppSettings>>();
            _appSettingsPersistor.Instance.Returns(_appSettings);

            _settingsViewModel = new SettingsViewModel(new AppContracts(),
                _coverService, _logger, _hotkeyViewModel, _spotifyController, _autorunService, _appSettingsPersistor);
        }

        [Fact]
        public void TestAlwaysOnTop()
        {
            _appSettings.AlwaysOnTop.Returns(false, true);
            Assert.False(_settingsViewModel.AlwaysOnTop);
            Assert.True(_settingsViewModel.AlwaysOnTop);
        }

        [Fact]
        public void TestStartWithWindows()
        {
            _autorunService.DidNotReceive().ValidateAutorun();

            Assert.PropertyChanged(_settingsViewModel, 
                nameof(_settingsViewModel.StartWithWindows),
                () => _settingsViewModel.StartWithWindows = true);

            Assert.True(_settingsViewModel.StartWithWindows);
            _appSettings.Received(1).StartWithWindows = true;
            _autorunService.Received(1).ValidateAutorun();


            _appSettings.ClearReceivedCalls();
            _autorunService.ClearReceivedCalls();

            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.StartWithWindows),
                () => _settingsViewModel.StartWithWindows = false);

            Assert.False(_settingsViewModel.StartWithWindows);
            _appSettings.Received(1).StartWithWindows = false;
            _autorunService.Received(1).ValidateAutorun();


            //Test getters
            _appSettings.StartWithWindows.Returns(true, false);
            Assert.Equal(true, _settingsViewModel.StartWithWindows);
            Assert.Equal(false, _settingsViewModel.StartWithWindows);
        }

        [Fact]
        public void TestHideIfSpotifyClosed()
        {
            _appSettings.DidNotReceiveWithAnyArgs().HideIfSpotifyClosed = true;

            Assert.PropertyChanged(_settingsViewModel, 
                nameof(_settingsViewModel.HideIfSpotifyClosed), 
                () => { _settingsViewModel.HideIfSpotifyClosed = true; });
            _appSettings.Received(1).HideIfSpotifyClosed = true;


            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.HideIfSpotifyClosed),
                () => { _settingsViewModel.HideIfSpotifyClosed = false; });
            _appSettings.Received(1).HideIfSpotifyClosed = false;

            //Test getters
            _appSettings.HideIfSpotifyClosed.Returns(true, false);
            Assert.Equal(true, _settingsViewModel.HideIfSpotifyClosed);
            Assert.Equal(false, _settingsViewModel.HideIfSpotifyClosed);
        }

        [Fact]
        public void TestDisableAnimations()
        {
            _appSettings.DidNotReceiveWithAnyArgs().DisableAnimations = true;

            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.DisableAnimations),
                () => { _settingsViewModel.DisableAnimations = true; });
            _appSettings.Received(1).DisableAnimations = true;


            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.DisableAnimations),
                () => { _settingsViewModel.DisableAnimations = false; });
            _appSettings.Received(1).DisableAnimations = false;

            //Test getters
            _appSettings.DisableAnimations.Returns(true, false);
            Assert.Equal(true, _settingsViewModel.DisableAnimations);
            Assert.Equal(false, _settingsViewModel.DisableAnimations);
        }

        [Fact]
        public void TestStartMinimized()
        {
            _appSettings.DidNotReceiveWithAnyArgs().StartMinimized = true;

            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.StartMinimized),
                () => { _settingsViewModel.StartMinimized = true; });
            _appSettings.Received(1).StartMinimized = true;


            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.StartMinimized),
                () => { _settingsViewModel.StartMinimized = false; });
            _appSettings.Received(1).StartMinimized = false;

            //Test getters
            _appSettings.StartMinimized.Returns(true, false);
            Assert.Equal(true, _settingsViewModel.StartMinimized);
            Assert.Equal(false, _settingsViewModel.StartMinimized);
        }

        [Fact]
        public void TestUpdatePreference()
        {
            _appSettings.DidNotReceiveWithAnyArgs().UpdatePreference = UpdatePreference.Stable;

            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.UpdatePreference),
                () => { _settingsViewModel.UpdatePreference = UpdatePreference.Stable; });
            _appSettings.Received(1).UpdatePreference = UpdatePreference.Stable;


            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.UpdatePreference),
                () => { _settingsViewModel.UpdatePreference = UpdatePreference.Developer; });
            _appSettings.Received(1).UpdatePreference = UpdatePreference.Developer;

            //Test getters
            _appSettings.UpdatePreference.Returns(UpdatePreference.Developer, UpdatePreference.Stable);
            Assert.Equal(UpdatePreference.Developer, _settingsViewModel.UpdatePreference);
            Assert.Equal(UpdatePreference.Stable, _settingsViewModel.UpdatePreference);
        }

        [Fact]
        public void TestSingleClickHide()
        {
            _appSettings.DidNotReceiveWithAnyArgs().SingleClickHide = true;

            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.SingleClickHide),
                () => { _settingsViewModel.SingleClickHide = true; });
            _appSettings.Received(1).SingleClickHide = true;


            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.SingleClickHide),
                () => { _settingsViewModel.SingleClickHide = false; });
            _appSettings.Received(1).SingleClickHide = false;

            //Test getters
            _appSettings.SingleClickHide.Returns(true, false);
            Assert.Equal(true, _settingsViewModel.SingleClickHide);
            Assert.Equal(false, _settingsViewModel.SingleClickHide);
        }

        [Fact]
        public void TestLanguage()
        {
            _appSettings.DidNotReceiveWithAnyArgs().Language = LanguageHelper.Russian;

            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.Language),
                () => { _settingsViewModel.Language = LanguageHelper.Russian; });
            _appSettings.Received(1).Language = LanguageHelper.Russian;
            Assert.Equal(LanguageHelper.Russian.CultureInfo, Thread.CurrentThread.CurrentCulture);
            Assert.Equal(LanguageHelper.Russian.CultureInfo, Thread.CurrentThread.CurrentUICulture);


            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.Language),
                () => { _settingsViewModel.Language = LanguageHelper.English; });
            _appSettings.Received(1).Language = LanguageHelper.English;
            Assert.Equal(LanguageHelper.English.CultureInfo, Thread.CurrentThread.CurrentCulture);
            Assert.Equal(LanguageHelper.English.CultureInfo, Thread.CurrentThread.CurrentUICulture);

            //Test getters
            _appSettings.Language.Returns(LanguageHelper.Russian, LanguageHelper.English);
            Assert.Equal(LanguageHelper.Russian, _settingsViewModel.Language);
            Assert.Equal(LanguageHelper.English, _settingsViewModel.Language);
        }

        [Fact]
        public void TestLockScreenBehavior()
        {
            _appSettings.DidNotReceiveWithAnyArgs().LockScreenBehavior = LockScreenBehavior.Pause;

            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.LockScreenBehavior),
                () => { _settingsViewModel.LockScreenBehavior = LockScreenBehavior.Pause; });
            _appSettings.Received(1).LockScreenBehavior = LockScreenBehavior.Pause;


            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.LockScreenBehavior),
                () => { _settingsViewModel.LockScreenBehavior = LockScreenBehavior.PauseUnpauseAlways; });
            _appSettings.Received(1).LockScreenBehavior = LockScreenBehavior.PauseUnpauseAlways;

            //Test getters
            _appSettings.LockScreenBehavior.Returns(LockScreenBehavior.PauseUnpauseAlways, LockScreenBehavior.Pause);
            Assert.Equal(LockScreenBehavior.PauseUnpauseAlways, _settingsViewModel.LockScreenBehavior);
            Assert.Equal(LockScreenBehavior.Pause, _settingsViewModel.LockScreenBehavior);
        }


        [Fact]
        public void TestCanClearCache()
        {
            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.CanClearCache),
                () => { _settingsViewModel.CanClearCache = false; });

            Assert.False(_settingsViewModel.CanClearCache);           

            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.CanClearCache),
                () => { _settingsViewModel.CanClearCache = true; });
            Assert.True(_settingsViewModel.CanClearCache);
        }

        [Fact]
        public void TestApplicationSize()
        {
            _appSettings.DidNotReceiveWithAnyArgs().ApplicationSize = ApplicationSize.Medium;

            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.ApplicationSize),
                () => { _settingsViewModel.ApplicationSize = ApplicationSize.Medium; });
            _appSettings.Received(1).ApplicationSize = ApplicationSize.Medium;


            Assert.PropertyChanged(_settingsViewModel,
                nameof(_settingsViewModel.ApplicationSize),
                () => { _settingsViewModel.ApplicationSize = ApplicationSize.Large; });
            _appSettings.Received(1).ApplicationSize = ApplicationSize.Large;

            //Test getters
            _appSettings.ApplicationSize.Returns(ApplicationSize.Medium, ApplicationSize.Large);
            Assert.Equal(ApplicationSize.Medium, _settingsViewModel.ApplicationSize);
            Assert.Equal(ApplicationSize.Large, _settingsViewModel.ApplicationSize);
        }

        [Fact]
        public async Task TestClearCache()
        {
            _coverService.CacheSize().Returns(1000);
            Assert.True(_settingsViewModel.CanClearCache);

            await _settingsViewModel.ClearCache();
            _coverService.Received(1).ClearCache();
            Assert.Equal("1000.00 B", _settingsViewModel.CacheSize);

            Assert.True(_settingsViewModel.CanClearCache);
        }

        [Fact]
        public async Task TestClearCacheFailed()
        {
            Assert.True(_settingsViewModel.CanClearCache);
            var exception = new Exception("Boom");
            _coverService.When(x => x.ClearCache()).Do(info => { throw exception; });

            await _settingsViewModel.ClearCache();
            _logger.Received(1).WarnException("Failed to clear cover cache", exception);

            Assert.True(_settingsViewModel.CanClearCache);
        }

        [Fact]
        public void TestHotkeyViewModel()
        {
            Assert.Equal(_hotkeyViewModel, _settingsViewModel.HotKeyViewModel);
        }

        [Fact]
        public async Task TestUpdateLogin()
        {
            Assert.False(_settingsViewModel.LoginChecking);
            Assert.False(_settingsViewModel.LoggedIn);

            _spotifyController.IsUserLoggedIn().Returns(info =>
            {
                Assert.True(_settingsViewModel.LoginChecking);
                return Task.FromResult(true);
            });
            await _settingsViewModel.UpdateLoggedIn();
            Assert.False(_settingsViewModel.LoginChecking);
            Assert.True(_settingsViewModel.LoggedIn);
        }

        [Fact]
        public void TestLanguages()
        {
            Assert.Collection(_settingsViewModel.Languages, 
                l => Assert.Equal(l, LanguageHelper.English), 
                l => Assert.Equal(l, LanguageHelper.Russian));
        }
    }
}
