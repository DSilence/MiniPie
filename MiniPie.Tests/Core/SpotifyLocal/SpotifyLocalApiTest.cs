using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using MiniPie.Core;
using MiniPie.Core.SpotifyLocal;
using NSubstitute;
using Xunit;

namespace MiniPie.Tests.Core.SpotifyLocal
{
    public class SpotifyLocalApiTest
    {
        private FakeResponseHandler _fakeResponseHandler = new FakeResponseHandler();
        private SpotifyLocalApi _localApi = new SpotifyLocalApi(Substitute.For<ILog>(), new AppContracts());

        public SpotifyLocalApiTest()
        {
            typeof (SpotifyLocalApi).GetField("_client", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_localApi, new HttpClient(_fakeResponseHandler));
        }

        private const string ValidStatus = "{\n\"version\": 9, \n\"client_version\": \"1.0.21.143.g76c19bcd\", \n\"playing\": true, \n\"shuffle\": false, \n\"repeat\": false, \n\"play_enabled\": true, \n\"prev_enabled\": true, \n\"next_enabled\": true, \n\"track\": {\n  \"track_resource\": {\n    \"name\": \"Red Eyes\", \n    \"uri\": \"spotify:track:021SGIeEW1aG9xvgGA9a9J\", \n    \"location\": {\n      \"og\": \"https://open.spotify.com/track/021SGIeEW1aG9xvgGA9a9J\"\n    }\n  }, \n  \"artist_resource\": {\n    \"name\": \"Thomas Azier\", \n    \"uri\": \"spotify:artist:6AE7CSJUwDMnTXV4yKVLLv\", \n    \"location\": {\n      \"og\": \"https://open.spotify.com/artist/6AE7CSJUwDMnTXV4yKVLLv\"\n    }\n  }, \n  \"album_resource\": {\n    \"name\": \"Hylas\", \n    \"uri\": \"spotify:album:5A5Yqw6DIP1lCmzQjA79I0\", \n    \"location\": {\n      \"og\": \"https://open.spotify.com/album/5A5Yqw6DIP1lCmzQjA79I0\"\n    }\n  }, \n  \"length\": 212, \n  \"track_type\": \"normal\"\n}, \n\"context\": {\n}, \n\"playing_position\": 28.096, \n\"server_time\": 1455023441, \n\"volume\": 0.70998704, \n\"online\": true, \n\"open_graph_state\": {\n  \"private_session\": false, \n  \"posting_disabled\": false\n}, \n\"running\": true\n}";

        [Fact]
        public async Task TestPlay()
        {
            _fakeResponseHandler.AddFakeResponse(new Uri("http://minipie.spotilocal.com:4380/remote/play.json?uri=&oauth=&csrf="), new HttpResponseMessage
            {
                Content = new StringContent(ValidStatus)
            });
            var result = await _localApi.Play();
            Assert.NotNull(result);
            Assert.Equal(9, result.version);
        }

        [Fact]
        public async Task TestPause()
        {
            _fakeResponseHandler.AddFakeResponse(new Uri("http://minipie.spotilocal.com:4380/remote/pause.json?pause=true&oauth=&csrf="), new HttpResponseMessage
            {
                Content = new StringContent(ValidStatus)
            });
            var result = await _localApi.Pause();
            Assert.NotNull(result);
            Assert.Equal(9, result.version);
        }

        [Fact]
        public async Task TestResume()
        {
            _fakeResponseHandler.AddFakeResponse(new Uri("http://minipie.spotilocal.com:4380/remote/pause.json?pause=false&oauth=&csrf="), new HttpResponseMessage
            {
                Content = new StringContent(ValidStatus)
            });
            var result = await _localApi.Resume();
            Assert.NotNull(result);
            Assert.Equal(9, result.version);
        }

        [Fact]
        public async Task TestQueue()
        {
            string songToQueue = "123";
            _fakeResponseHandler.AddFakeResponse(new Uri("http://minipie.spotilocal.com:4380/remote/play.json?uri=123%3faction%3dqueue&oauth=&csrf="), new HttpResponseMessage
            {
                Content = new StringContent(ValidStatus)
            });
            var result = await _localApi.Queue(songToQueue);
            Assert.NotNull(result);
            Assert.Equal(9, result.version);
        }

        private const string ValidTokenResponse = "{\n\"token\": \"e45470f550ef278832f2ae96b7af7473\"\n}";
        [Fact]
        public async Task TestGetCfid()
        {
            _fakeResponseHandler.AddFakeResponse(new Uri("http://minipie.spotilocal.com:4380/simplecsrf/token.json"),
                new HttpResponseMessage
                {
                    Content = new StringContent(ValidTokenResponse)
                });
            var result = await _localApi.GetCfid();
            Assert.Equal("e45470f550ef278832f2ae96b7af7473", result.token);
        }
    }
}
