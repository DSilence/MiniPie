using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MiniPie.Core;
using MiniPie.Core.SpotifyLocal;
using MiniPie.Core.SpotifyWeb;
using NSubstitute;
using Xunit;
using Track = MiniPie.Core.SpotifyLocal.Track;

namespace MiniPie.Tests
{
    public class CoverServiceTest: IDisposable
    {
        private readonly CoverService _coverService;
        private readonly string CoverTestDirectoryName = Path.GetRandomFileName();
        private readonly ISpotifyWebApi _webApi = Substitute.For<ISpotifyWebApi>();
        private readonly ILog _log = Substitute.For<ILog>();

        public CoverServiceTest()
        {
            Directory.CreateDirectory(CoverTestDirectoryName);
            _coverService = new CoverService(CoverTestDirectoryName, _log, _webApi);
        }

        [Fact]
        public void TestCacheSizeAndClear()
        {
            var file1 = new byte[] {1,2,3};
            var file2 = new byte[] {1,2};
            File.WriteAllBytes(Path.Combine(CoverTestDirectoryName, "CoverCache", "file1.jpg"), file1);
            File.WriteAllBytes(Path.Combine(CoverTestDirectoryName, "CoverCache","file2.jpg"), file2);

            Assert.Equal(5, _coverService.CacheSize());

            _coverService.ClearCache();
            Assert.Equal(0, _coverService.CacheSize());
            Assert.Equal(0, Directory.EnumerateFiles(CoverTestDirectoryName).Count());
        }

        [Fact]
        public async Task TestFetchCoverWebApiSource()
        {
            string uri = "test_uri";
            string download_uri = "https://i.scdn.co/image/cbdd47d8d6abe7c54d58e0583dfbddbdee703ed2";
            var status = new Status
            {
                track = new Track
                {
                    track_resource = new Resource
                    {
                        uri = uri
                    }
                }
            };

            var webException = new WebException("test");
            var exception = new Exception();
            _webApi.GetTrackArt(uri).Returns(
                info => null,
                info => { throw webException; },
                info => { throw exception; },
                info => download_uri);
            Assert.Equal(string.Empty, await _coverService.FetchCover(status));
            Assert.Equal(string.Empty, await _coverService.FetchCover(status));
            _log.Received(1).WarnException("Failed to retrieve Image via Spotify Local API: test", webException);

            Assert.Equal(string.Empty, await _coverService.FetchCover(status));
            _log.Received(1).WarnException("Failed to retrieve cover from Spotify", exception);

            var result = await _coverService.FetchCover(status);
            await _webApi.Received(4).GetTrackArt(uri);
            _webApi.ClearReceivedCalls();
            var result2 = await _coverService.FetchCover(status);
            //Used cached value
            await _webApi.Received(0).GetTrackArt(uri);
            Assert.Equal(CoverTestDirectoryName + "\\CoverCache\\a83c1a688201fcf3e304493ed0425fe602e45eb8.jpg", result);
            Assert.Equal(result, result2);
            
        }

        [Fact]
        public async Task TestFetchLocalCover()
        {
            string uri = "spotify:local:test_uri";
            string download_uri = "https://i.scdn.co/image/cbdd47d8d6abe7c54d58e0583dfbddbdee703ed2";
            string found_uri = "test_uri";

            var status = new Status
            {
                track = new Track
                {
                    track_resource = new Resource
                    {
                        uri = uri,
                        name = "test"
                    },
                    artist_resource = new Resource
                    {
                        name = "test"
                    }
                }
            };
            var exception = new Exception();
            _webApi.TrackSearch(null)
                .ReturnsForAnyArgs(info => new MiniPie.Core.SpotifyWeb.Models.Track[0], info => { throw exception; }, info => new[] {new MiniPie.Core.SpotifyWeb.Models.Track {Uri = found_uri} });
            _webApi.GetTrackArt(found_uri).Returns(download_uri);

            var result = await _coverService.FetchCover(status);
            Assert.Equal("", result);
            result = await _coverService.FetchCover(status);
            Assert.Equal("", result);
            result = await _coverService.FetchCover(status);
            Assert.True(result.Contains("CoverCache\\33606d2b155398cf4f1b458f3565037e07206916.jpg"));
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(CoverTestDirectoryName))
                {
                    Directory.Delete(CoverTestDirectoryName, true);
                }
            }
            catch (Exception)
            {
                Thread.Sleep(500);
                if (Directory.Exists(CoverTestDirectoryName))
                {
                    Directory.Delete(CoverTestDirectoryName, true);
                }
            }
        }
    }
}