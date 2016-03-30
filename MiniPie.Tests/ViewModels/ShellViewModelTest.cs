using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Caliburn.Micro;
using MiniPie.Core;
using MiniPie.Core.SpotifyLocal;
using MiniPie.Core.SpotifyWeb.Models;
using MiniPie.Manager;
using MiniPie.ViewModels;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SimpleInjector;
using Xunit;
using ILog = MiniPie.Core.ILog;
using Track = MiniPie.Core.SpotifyLocal.Track;

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

        [Fact]
        public async Task TestUpdateViewSuccess()
        {
            var status = new Status
            {
                track = new Track
                {
                    track_resource = new Resource
                    {
                        name = "TestName",
                        uri = "SpotifyUri",
                        location = new Location
                        {
                            og = "https://open.spotify.com/track/trackurl"
                        }
                    },
                    artist_resource = new Resource
                    {
                        name = "TestArtistName"
                    }
                },
                playing = true
            };
            _spotifyController.IsSpotifyOpen().Returns(true);
            _spotifyController.GetStatus().Returns(status);
            _settings.SpotifyToken = new MiniPie.Core.SpotifyWeb.Token();
            _coverService.FetchCover(status).Returns("TestCover");
            _spotifyController.IsTracksSaved(null).ReturnsForAnyArgs(new[] {true});
            _spotifyController.GetPlaylists().Returns(new List<Playlist>
            {
                new Playlist
                {
                    Id = "TestId"
                }
            });

            await _shellViewModel.UpdateView();
            Assert.Equal("TestName", _shellViewModel.CurrentTrack);
            Assert.Equal("SpotifyUri", _shellViewModel.SpotifyUri);
            Assert.Equal("https://open.spotify.com/track/trackurl", _shellViewModel.TrackUrl);
            Assert.Equal("TestArtistName – TestName", _shellViewModel.TrackFriendlyName);
            Assert.Equal("trackurl", _shellViewModel.TrackId);
            Assert.Equal("TestCover", _shellViewModel.CoverImage);

            Assert.Collection(_shellViewModel.Playlists, playlist => Assert.Equal("TestId", playlist.Id));
        }

        [Fact]
        public async Task TestUpdateViewCoverFailed()
        {
            var status = new Status
            {
                track = new Track
                {
                    track_resource = new Resource
                    {
                        name = "TestName",
                        uri = "SpotifyUri",
                        location = new Location
                        {
                            og = "https://open.spotify.com/track/trackurl"
                        }
                    },
                    artist_resource = new Resource
                    {
                        name = "TestArtistName"
                    }
                },
                playing = true
            };
            _spotifyController.IsSpotifyOpen().Returns(true);

             var exception = new Exception("Wow");
            _coverService.FetchCover(null).ThrowsForAnyArgs(exception);
            _settings.SpotifyToken = new MiniPie.Core.SpotifyWeb.Token();
            _spotifyController.GetStatus().Returns(status);

            await _shellViewModel.UpdateView();

            _log.Received(1).WarnException("Failed to retrieve cover information with: Wow", exception);
            await _spotifyController.ReceivedWithAnyArgs(1).IsTracksSaved(null);
        }

        [Fact]
        public async Task TestUpdateViewFailed()
        {
            var exception = new Exception("Failure");
            _spotifyController.GetStatus().Throws(exception);
            await _shellViewModel.UpdateView();
            _log.Received(1).FatalException("UpdateView() failed hard with: Failure", exception);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TestUpdateViewCanDoStuff(bool isOpen)
        {
            var status = new Status
            {
                track = new Track
                {
                    track_resource = new Resource
                    {
                        name = "TestName",
                        uri = "SpotifyUri",
                        location = new Location
                        {
                            og = "open.spotify.com/track/TrackUrl"
                        }
                    },
                    artist_resource = new Resource
                    {
                        name = "TestArtistName"
                    }

                }
            };
            _spotifyController.GetStatus().Returns(status);
            _spotifyController.IsSpotifyOpen().Returns(isOpen);

            await _shellViewModel.UpdateView();
           
            Assert.Equal(isOpen, _shellViewModel.CanPlayNext);
            Assert.Equal(isOpen, _shellViewModel.CanPlayPrevious);
            Assert.Equal(isOpen, _shellViewModel.CanPlayPause);
            Assert.Equal(isOpen, _shellViewModel.CanVolumeDown);
            Assert.Equal(isOpen, _shellViewModel.CanVolumeUp);
        }
    }
}