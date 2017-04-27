using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using MiniPie.Core.SpotifyWeb.Models;

namespace MiniPie.Core.SpotifyWeb
{
    public class SpotifyWebApi : ISpotifyWebApi
    {
        private SpotifyHttpClient _client;
        private readonly ILog _log;
        private const string SpotifyApiUrl = "https://api.spotify.com/v1/";
        private const string RedirectUrl = "minipie://callback";
        private readonly AppSettings _appSettings;
        private Timer _timer;

        private const int PlayListLimit = 50;
        #region Uris
        private const string _albumsUri = "albums/{0}";
        private const string SpotifyProfileUri = "me";
        private const string AddToPlaylistUrl = "users/{0}/playlists/{1}/tracks?uris={2}";
        private const string PlaylistsUrl = "me/playlists?limit={0}";
        private const string GetTrackInfoUrl = "tracks?ids={0}";
        private const string IsTrackSavedFormat = "me/tracks/contains?ids={0}";
        private const string MyMusicAddFormat = "me/tracks";
        private const string MyMusicDeleteFormat = MyMusicAddFormat + "?ids={0}";
        private const string TrackInfoUrl = "tracks/{0}";
        private const string TrackSearchUrl = "search?q={0}&type=track";
        #endregion

        public event EventHandler TokenUpdated;

        public SpotifyWebApi(ILog log, AppSettings settings)
        {
            _log = log;
            _appSettings = settings;
        }

        public virtual async Task Initialize()
        {
            _timer = new Timer(async state =>
            {
                await _client.Authenticate(_appSettings.SpotifyToken.RefreshToken, GrantType.RefreshToken).ConfigureAwait(false);
            });
            _client = new SpotifyHttpClient(SpotifyApiUrl, _log, RedirectUrl, _appSettings, token =>
            {
                TokenUpdated?.Invoke(this, null);
                _timer.Change((token.ExpiresIn - 15) * 1000, int.MaxValue);
            });
            if (_appSettings.SpotifyToken != null)
            {
                await _client.Authenticate(_appSettings.SpotifyToken.RefreshToken, GrantType.RefreshToken).ConfigureAwait(false);
            }
        }

        private const string Scope = "playlist-read-private playlist-read-collaborative playlist-modify-public playlist-modify playlist-modify-private user-library-read user-library-modify user-follow-modify user-follow-read streaming user-read-playback-state";
        private const string LoginQueryFormat =
            "https://accounts.spotify.com/authorize/?client_id={0}&response_type=code&redirect_uri={1}&state={2}&scope={3}";
        public Uri BuildLoginQuery()
        {
            var scopeEncoded = HttpUtility.UrlEncode(Scope);
            var loginUri = new Uri(
                string.Format(LoginQueryFormat, AppContracts.ClientId,
                RedirectUrl, Guid.NewGuid(), scopeEncoded));
            return loginUri;
        }
        
        public Task<User> GetProfile()
        {
            return _client.DoGetAsync<User>(SpotifyProfileUri);
        }

        public async Task<string> GetAlbumArt(string uri)
        {
            var albumId = uri.Split(new[] {":"}, StringSplitOptions.RemoveEmptyEntries).Last();
            var album = await _client.DoGetAsync<Models.Album>(string.Format(_albumsUri, albumId));
            if (album?.Images != null && album.Images.Count > 1)
            {
                return album.Images[1].Url;
            }
            throw new ApplicationException("Failed to retrieve art url");
        }

        public async Task<string> GetTrackArt(string uri)
        {
            var trackId = uri.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Last();
            var track = await _client.DoGetAsync<Models.Track>(string.Format(TrackInfoUrl, trackId));
            if (track?.Album?.Images != null && track.Album?.Images?.Count > 1)
            {
                return track.Album.Images[1].Url;
            }
            throw new ApplicationException("Failed to retrieve art url");
        }


        public async Task<IList<Playlist>> GetUserPlaylists()
        {
            var url = string.Format(PlaylistsUrl, PlayListLimit);
            var playLists = await _client.DoGetAsync<PagingObject<Playlist>>(url).ConfigureAwait(false);
            var result = playLists.Items.ToList();
            while (playLists.Next != null)
            {
                playLists = await _client.DoGetAsync<PagingObject<Playlist>>(playLists.Next).ConfigureAwait(false);
                result.AddRange(playLists.Items);
            }
            return result;
        }

        public async Task AddToPlaylist(string playlistId, string trackUrls)
        {
            var profile = await GetProfile();
            if (profile != null)
            {
                var url = string.Format(AddToPlaylistUrl, profile.Id, playlistId, trackUrls);
                await _client.DoPostAsync(url);
            }
        }

        public async Task<IList<Models.Track>> GetTrackInfo(IList<string> trackIds)
        {
            var url = string.Format(GetTrackInfoUrl, string.Join(",", trackIds));
            var tracks = await _client.DoGetAsync<TrackCollection>(url).ConfigureAwait(false);
            return tracks.Tracks;
        }
        public async Task<IList<Models.Track>> TrackSearch(string searchString)
        {
            var url = string.Format(TrackSearchUrl, searchString);
            var tracks = await _client.DoGetAsync<SearchResult>(url).ConfigureAwait(false);
            return tracks.Tracks.Items;
        }

        public void Logout()
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }

        public async Task CreateToken(string response)
        {
            var queryResult = HttpUtility.ParseQueryString(new Uri(response).Query);
            await _client.Authenticate(queryResult["code"]);
        }

        public Task<IList<bool>> IsTracksSaved(IList<string> trackIds)
        {
            var url = string.Format(IsTrackSavedFormat, string.Join(",", trackIds));
            return _client.DoGetAsync<IList<bool>>(url);
        }
        
        public Task AddToMyMusic(IList<string> trackIds)
        {
            var url = MyMusicAddFormat;
            return _client.DoPutAsync(url, trackIds);
        }

        public Task RemoveFromMyMusic(IList<string> trackIds)
        {
            var url = string.Format(MyMusicDeleteFormat, string.Join(",", trackIds));
            return _client.DoDeleteAsync(url);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
