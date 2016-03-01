using System;
using System.Security.RightsManagement;
using MiniPie.Core;
using MiniPie.Core.SpotifyLocal;
using MiniPie.Core.SpotifyNative;
using MiniPie.Core.SpotifyWeb;
using NSubstitute;
using Xunit;
using Track = MiniPie.Core.SpotifyLocal.Track;

namespace MiniPie.Tests.Controller
{
    public class SpotifyControllerTest: IDisposable
    {
        private SpotifyController _spotifyController;
        public SpotifyControllerTest()
        {
            var localApi = Substitute.For<ISpotifyLocalApi>();
            var spotifyWebApi = Substitute.For<ISpotifyWebApi>();
            var spotifyNativeApi = Substitute.For<ISpotifyNativeApi>();
            var log = Substitute.For<ILog>();

            _spotifyController = new SpotifyController(log, localApi, spotifyWebApi, spotifyNativeApi);
        }

        [Fact]
        public void ProcessTrackInfoTest()
        {
            bool trackChanged = false;
            bool trackTimerFiler = false;
            _spotifyController.TrackStatusChanged += (sender, args) =>
            {
                trackTimerFiler = true;
            };
            _spotifyController.TrackChanged += (sender, args) =>
            {
                trackChanged = true;
            };
            var status = new Status();
            status.track = new Track
            {
                track_resource = new Resource
                {
                    uri = "Awesome Track"
                }
            };
            _spotifyController.ProcessTrackInfo(status);
            Assert.True(trackChanged);
            
            _spotifyController.ProcessTrackInfo(status);
            Assert.True(trackTimerFiler);
        }

        [Fact]
        public void GetDelayForPlaybackUpdateTest()
        {
            double position1 = 12.43;
            double position2 = 139848934.3423240;
            Assert.Equal(_spotifyController.GetDelayForPlaybackUpdate(position1), 570);
            Assert.Equal(_spotifyController.GetDelayForPlaybackUpdate(position2), 657);
        }

        [Fact]
        public void GetStatusTest()
        {
            var status = new Status();
            _spotifyController.ProcessTrackInfo(status);
            Assert.Equal(status, _spotifyController.GetStatus());
        }

        public void Dispose()
        {
            _spotifyController.Dispose();
        }
    }
}
