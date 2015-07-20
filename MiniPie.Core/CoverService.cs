using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using MiniPie.Core.Extensions;
using MiniPie.Core.SpotifyLocal;

namespace MiniPie.Core {
    public class CoverService : ICoverService {

        private const string CacheFileNameTemplate = "{0}.jpg";
        private readonly string _CacheDirectory;
        private readonly SpotifyLocalApi _LocalApi;
        private readonly ILog _Logger;
        private HttpClient _client = new HttpClient();
        
        private const int MaxFileCount = 500;

        public CoverService(string cacheRootDirectory,
            ILog logger, SpotifyLocalApi localApi)
        {
            _CacheDirectory = Path.Combine(cacheRootDirectory, "CoverCache");
            _Logger = logger;
            _LocalApi = localApi;
            if (!Directory.Exists(_CacheDirectory))
                Directory.CreateDirectory(_CacheDirectory);
        }

        public long CacheSize()
        {
            return !Directory.Exists(_CacheDirectory)
                ? 0
                : new DirectoryInfo(_CacheDirectory).GetFiles().Sum(f => f.Length);
        }

        public void ClearCache() {
            Directory.GetFiles(_CacheDirectory,"*.jpg").ToList().ForEach(f => {
                                                                             try {
                                                                                 File.Delete(f);
                                                                             }
                                                                             catch (Exception exc) {
                                                                                 _Logger.WarnException("Failed to delete file", exc);
                                                                             }
                                                                         });
        }

        public async Task<string> FetchCover(Status trackStatus) {
            var cachedFileName = Path.Combine(_CacheDirectory, string.Format(CacheFileNameTemplate, (trackStatus.track.album_resource.uri).ToSHA1()));
            if (File.Exists(cachedFileName))
                return cachedFileName;

            var spotifyCover = await FetchSpotifyCover(trackStatus, cachedFileName);
            return spotifyCover;
        }

        private async Task<string> FetchSpotifyCover(Status trackStatus, string cachedFileName) {
            if (!_LocalApi.HasValidToken)
                return string.Empty;

            try {
                if (trackStatus != null) {
                    if (trackStatus.error != null)
                        throw new Exception(string.Format("API Error: {0} (0x{1})", trackStatus.error.message,
                                                          trackStatus.error.type));

                    if (trackStatus.track != null && trackStatus.track.album_resource != null) {
                        var coverUrl = await _LocalApi.GetArt(trackStatus.track.album_resource.uri);
                        if (!string.IsNullOrEmpty(coverUrl))
                            return await DownloadAndSaveImage(coverUrl, cachedFileName);
                    }
                }
            }
            catch (WebException webExc) {
                if(webExc.Response != null && ((HttpWebResponse)webExc.Response).StatusCode != HttpStatusCode.NotFound)
                    _Logger.WarnException(string.Format("Failed to retrieve Image via Spotify Local API: {0}", webExc.Message), webExc);
            }
            catch (Exception exc) {
                _Logger.WarnException("Failed to retrieve cover from Spotify", exc);
            }
            return string.Empty;
        }

        private async Task<string> DownloadAndSaveImage(string url, string destination)
        {
            using (var fs = File.Create(destination))
            {
                using (var rs = await _client.GetStreamAsync(url))
                {
                    await rs.CopyToAsync(fs);
                    CleanupCache();
                }
            }

            return destination;
        }

        private void CleanupCache()
        {
            Task.Run(() =>
            {
                DirectoryInfo info = new DirectoryInfo(_CacheDirectory);
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
            return;
        }
    }
}