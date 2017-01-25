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
    public class CoverService : ICoverService, IDisposable
    {
        private const string CacheFileNameTemplate = "{0}.jpg";
        private readonly string _cacheDirectory;
        private readonly ISpotifyWebApi _webApi;
        private readonly ILog _logger;
        private readonly WebClient _client = new WebClient();

        private readonly int MaxFileCount = 500;
        public static string CacheDirName = "CoverCache";

        public CoverService(string cacheRootDirectory,
            ILog logger, ISpotifyWebApi webApi)
        {
            _cacheDirectory = Path.Combine(cacheRootDirectory, CacheDirName);
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
            var trackUri = trackStatus.track.track_resource.uri;
            var sha = trackUri.ToSHA1();
            
            var cachedFileName = Path.Combine(_cacheDirectory,
                string.Format(CacheFileNameTemplate, sha));
            if (File.Exists(cachedFileName))
                return cachedFileName;

            if (trackUri.StartsWith("spotify:local", StringComparison.OrdinalIgnoreCase))
            {
                trackUri = await FetchRemoteSpotifyTrackFromLocal(trackStatus).ConfigureAwait(false);
            }
            var spotifyCover = await FetchSpotifyCover(trackUri, cachedFileName);
            return spotifyCover;
        }

        /// <summary>
        /// Attempts to retrieve remove spotify uri. Returns local uri (same as passed) if fails
        /// </summary>
        /// <param name="trackStatus"></param>
        /// <returns></returns>
        private async Task<string> FetchRemoteSpotifyTrackFromLocal(Status trackStatus)
        {
            try
            {
                var tracks =
                    await
                        _webApi.TrackSearch(
                            $"{trackStatus.track.artist_resource.name} {trackStatus.track.track_resource.name}")
                            .ConfigureAwait(false);
                if (tracks.Any())
                {
                    return tracks.First().Uri;
                }
            }
            catch (Exception e)
            {
                _logger.WarnException("Could not process replacement track cover for local track. Using default image. Exception is: " + e.Message, e);
            }
            return trackStatus.track.track_resource.uri;
        }

        private async Task<string> FetchSpotifyCover(string trackUri, string cachedFileName)
        {
            try
            {
                if (trackUri != null)
                {
                    var coverUrl = await _webApi.GetTrackArt(trackUri);
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
            await CleanupCache().ConfigureAwait(false);

            return destination;
        }

        private Task CleanupCache()
        {
            return Task.Run(() =>
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
            });
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}