using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MiniPie.Core.Extensions;
using MiniPie.Core.SpotifyLocal;
using MiniPie.Core.SpotifyWeb;

namespace MiniPie.Core
{
    public class CoverService : ICoverService
    {
        private const string CacheFileNameTemplate = "{0}.jpg";
        private readonly string _cacheDirectory;
        private readonly ISpotifyWebApi _webApi;
        private readonly ILog _logger;
        private readonly WebClient _client = new WebClient();

        private readonly int MaxFileCount = 500;

        public CoverService(string cacheRootDirectory,
            ILog logger, ISpotifyWebApi webApi)
        {
            _cacheDirectory = Path.Combine(cacheRootDirectory, "CoverCache");
            _logger = logger;
            _webApi = webApi;
            if (!Directory.Exists(_cacheDirectory))
                Directory.CreateDirectory(_cacheDirectory);
        }

        public long CacheSize()
        {
            return !Directory.Exists(_cacheDirectory)
                ? 0
                : new DirectoryInfo(_cacheDirectory).GetFiles().Sum(f => f.Length);
        }

        public void ClearCache()
        {
            Directory.GetFiles(_cacheDirectory, "*.jpg").ToList().ForEach(f =>
            {
                try
                {
                    File.Delete(f);
                }
                catch (Exception exc)
                {
                    _logger.WarnException("Failed to delete file", exc);
                }
            });
        }

        public async Task<string> FetchCover(Status trackStatus)
        {
            var cachedFileName = Path.Combine(_cacheDirectory,
                string.Format(CacheFileNameTemplate, trackStatus.track.track_resource.uri.ToSHA1()));
            if (File.Exists(cachedFileName))
                return cachedFileName;

            var spotifyCover = await FetchSpotifyCover(trackStatus, cachedFileName);
            return spotifyCover;
        }

        private async Task<string> FetchSpotifyCover(Status trackStatus, string cachedFileName)
        {
            try
            {
                if (trackStatus.track?.track_resource != null)
                {
                    var coverUrl = await _webApi.GetTrackArt(trackStatus.track.track_resource.uri);
                    if (!string.IsNullOrEmpty(coverUrl))
                        return await DownloadAndSaveImage(coverUrl, cachedFileName);
                }
            }
            catch (WebException webExc)
            {
                _logger.WarnException(
                    string.Format("Failed to retrieve Image via Spotify Local API: {0}", webExc.Message), webExc);
            }
            catch (Exception exc)
            {
                _logger.WarnException("Failed to retrieve cover from Spotify", exc);
            }
            return string.Empty;
        }

        private async Task<string> DownloadAndSaveImage(string url, string destination)
        {
            await _client.DownloadFileTaskAsync(url, destination).ConfigureAwait(false);
            CleanupCache();

            return destination;
        }

        private async void CleanupCache()
        {
            await Task.Run(() =>
            {
                DirectoryInfo info = new DirectoryInfo(_cacheDirectory);
                IEnumerable<FileInfo> files = info.GetFiles();
                while (files.Count() > MaxFileCount)
                {
                    files = files.OrderBy(f => f.LastAccessTime);
                    if (!files.Any())
                    {
                        throw new Exception("Invalid number of files in cache");
                    }
                    else
                    {
                        files.First().Delete();
                        files = info.GetFiles();
                    }
                }
            }).ConfigureAwait(false);
        }
    }
}