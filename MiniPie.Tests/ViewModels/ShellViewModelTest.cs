using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using MiniPie.Core;
using MiniPie.Manager;
using MiniPie.ViewModels;
using NSubstitute;
using SimpleInjector;
using Xunit;
using ILog = MiniPie.Core.ILog;

namespace MiniPie.Tests.ViewModels
{
    public class ShellViewModelTest
    {
        private ShellViewModel _shellViewModel;
        private readonly IWindowManager _windowManager = Substitute.For<IWindowManager>();
        private readonly ISpotifyController _spotifyController = Substitute.For<ISpotifyController>();
        private readonly ICoverService _coverService = Substitute.For<ICoverService>();
        private readonly AppSettings _settings = new AppSettings();
        private readonly ILog _log = Substitute.For<ILog>();
        private readonly Container _container = Substitute.For<Container>();
        private readonly ClipboardManager _clipboardManager = Substitute.For<ClipboardManager>();

        public ShellViewModelTest()
        {
            _shellViewModel = new ShellViewModel(_windowManager, _spotifyController, _coverService, _settings, _log,
                _container, _clipboardManager);
        }

        [Fact]
        public async Task TestPlayPause()
        {
            _shellViewModel.CanPlayPause = false;
            await _shellViewModel.PlayPause();
            await _spotifyController.DidNotReceive().PausePlay();

            _shellViewModel.CanPlayPause = true;
            await _shellViewModel.PlayPause();
            await _spotifyController.Received(1).PausePlay();
        }

        [Fact]
        public async Task TestPlayPrevious()
        {
            _shellViewModel.CanPlayPrevious = false;
            await _shellViewModel.PlayPrevious();
            await _spotifyController.DidNotReceive().PreviousTrack();

            _shellViewModel.CanPlayPrevious = true;
            await _shellViewModel.PlayPrevious();
            await _spotifyController.Received(1).PreviousTrack();
        }

        [Fact]
        public async Task TestPlayNext()
        {
            _shellViewModel.CanPlayNext = false;
            await _shellViewModel.PlayNext();
            await _spotifyController.DidNotReceive().NextTrack();

            _shellViewModel.CanPlayNext = true;
            await _shellViewModel.PlayNext();
            await _spotifyController.Received(1).NextTrack();
        }

        [Fact]
        public async Task TestVolumeUp()
        {
            _shellViewModel.CanVolumeUp = false;
            await _shellViewModel.VolumeUp();
            await _spotifyController.DidNotReceive().VolumeUp();

            _shellViewModel.CanVolumeUp = true;
            await _shellViewModel.VolumeUp();
            await _spotifyController.Received(1).VolumeUp();
        }

        [Fact]
        public async Task TestVolumeDown()
        {
            _shellViewModel.CanVolumeDown = false;
            await _shellViewModel.VolumeDown();
            await _spotifyController.DidNotReceive().VolumeDown();

            _shellViewModel.CanVolumeDown = true;
            await _shellViewModel.VolumeDown();
            await _spotifyController.Received(1).VolumeDown();
        }

        [Fact]
        public void TestOpenSpotify()
        {
            _shellViewModel.OpenSpotifyWindow();
            _spotifyController.Received(1).OpenSpotify();
        }

        [Fact]
        public void TestCopyTrackName()
        {
            _shellViewModel.TrackFriendlyName = "Test0";
            _shellViewModel.CopyTrackName();
            _clipboardManager.Received(1).SetText("Test0");
        }

        [Fact]
        public void TestCopySpotifyName()
        {
            _shellViewModel.TrackUrl = "Test1";
            _shellViewModel.CopySpotifyLink();
            _clipboardManager.Received(1).SetText("Test1");
        }

        [Fact(Skip = "Not possible to change mock visibility")]
        public void TestHandleTrayMouseDoubleClick()
        {
            _settings.SingleClickHide = false;
            var testWindow = Substitute.For<UIElement>();
            _shellViewModel.HandleTrayMouseDoubleClick(testWindow);
            Assert.Equal(Visibility.Hidden, testWindow.Visibility);

            _settings.SingleClickHide = true;
            _shellViewModel.HandleTrayMouseDoubleClick(testWindow);
            Assert.Equal(Visibility.Visible, testWindow.Visibility);
            _shellViewModel.HandleTrayMouseDoubleClick(testWindow);
            Assert.Equal(Visibility.Hidden, testWindow.Visibility);
        }

        [Fact(Skip = "Not possible to change mock visibility")]
        public void TestHandleTrayMouseClick()
        {
            _settings.SingleClickHide = false;
            var testWindow = Substitute.For<UIElement>();
            _shellViewModel.HandleTrayMouseClick(testWindow);
            Assert.Equal(Visibility.Visible, testWindow.Visibility);

            _settings.SingleClickHide = true;
            _shellViewModel.HandleTrayMouseClick(testWindow);
            Assert.Equal(Visibility.Hidden, testWindow.Visibility);
            _shellViewModel.HandleTrayMouseClick(testWindow);
            Assert.Equal(Visibility.Visible, testWindow.Visibility);
        }
    }
}