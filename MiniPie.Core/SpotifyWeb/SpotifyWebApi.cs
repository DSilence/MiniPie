﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using MiniPie.Core.SpotifyWeb.Models;
using Newtonsoft.Json;

namespace MiniPie.Core.SpotifyWeb
{
    //TODO move all web api associated stuff here(album art, etc.)
    public class SpotifyWebApi
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly HttpClient _authClient = new HttpClient();
        private readonly ILog _log;
        private const string _spotifyApiUrl = "https://api.spotify.com/v1/";
        private const string _redirectUrl = "minipie://callback";
        private Token _spotifyToken;
        private AppSettings _appSettings;
        private Timer _timer;

        public event EventHandler TokenUpdated;

        public SpotifyWebApi(ILog log, AppSettings settings)
        {
            this._log = log;
            _appSettings = settings;
            _timer = new Timer(Callback);
        }

        public async Task Initialize()
        {
            if (_appSettings.SpotifyToken != null)
            {
                await UpdateToken(_appSettings.SpotifyToken.RefreshToken, GrantType.RefreshToken);
            }
        }

        private async void Callback(object state)
        {
            await UpdateToken(_appSettings.SpotifyToken.RefreshToken, GrantType.RefreshToken);
        }

        private readonly Uri _albumsUri = new Uri(_spotifyApiUrl + "albums/");


        private const string _spotifyProfileUri = _spotifyApiUrl + "me";
        public async Task<User> GetProfile()
        {
            var response = await _client.GetStringAsync(_spotifyProfileUri);
            User user = await JsonConvert.DeserializeObjectAsync<User>(response);
            return user;
        }

        public async Task<string> GetArt(string uri)
        {
            try
            {
                var albumId = uri.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Last();
                //Modified to use spotify WEB API
                var lines = await _client.GetStringAsync(new Uri(_albumsUri, albumId));
                var album = await JsonConvert.DeserializeObjectAsync<Models.Album>(lines);
                return album.Images[1].Url;
            }
            catch (WebException webException)
            {
                if (webException.Response != null && ((HttpWebResponse)webException.Response).StatusCode != HttpStatusCode.NotFound)
                    _log.WarnException("[WebException] Failed to retrieve cover url from Spotify", webException);
            }
            catch (Exception exc)
            {
                _log.WarnException("Failed to retrieve cover url from Spotify", exc);
            }
            return string.Empty;
        }

        private const int PlayListLimit = 50;
        private const string PlaylistsUrl = _spotifyApiUrl + "users/{0}/playlists?limit={1}";
        public async Task<IList<Playlist>> GetUserPlaylists()
        {
            try
            {
                var profile = await GetProfile();
                var url = string.Format(PlaylistsUrl, profile.Id, PlayListLimit);
                var stringResult = await _client.GetStringAsync(url);
                var playLists = JsonConvert.DeserializeObject<PagingObject<Playlist>>(stringResult);
                var result = playLists.Items.ToList();
                while (playLists.Next != null)
                {
                    stringResult = await _client.GetStringAsync(playLists.Next);
                    playLists = JsonConvert.DeserializeObject<PagingObject<Playlist>>(stringResult);
                    result.AddRange(playLists.Items);
                }
                return result;
            }
            catch (Exception ex)
            {
                _log.WarnException("Failed to retrieve User Playlists", ex);
            }
            return null;
        }

        private const string loginQueryFormat =
            "https://accounts.spotify.com/authorize/?client_id={0}&response_type=code&redirect_uri={1}&state={2}";
        public Uri BuildLoginQuery()
        {
            var loginUri = new Uri(
                string.Format(loginQueryFormat, AppContracts.ClientId, 
                _redirectUrl, Guid.NewGuid()));
            return loginUri;
        }

        public void Logout()
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }

        private const string tokenQueryFormat = "https://accounts.spotify.com/api/token";

        public async Task CreateToken(string response)
        {
            var queryResult = HttpUtility.ParseQueryString(new Uri(response).Query);
            await UpdateToken(queryResult["code"]);
        }

        public async Task UpdateToken(string refreshToken, 
            GrantType grantType = GrantType.AuthorizationCode)//string grantType="authorization_code")
        {
            try
            {
                _log.Info("Refreshing token");
                _log.Info("Token type:" + grantType.GetDescription());
                if (refreshToken == null)
                {
                    return;
                }
                var parameters = new Dictionary<string, string>();
                parameters.Add("grant_type", grantType.GetDescription());
                parameters.Add("redirect_uri", _redirectUrl);
                parameters.Add("client_id", AppContracts.ClientId);
                parameters.Add("client_secret", AppContracts.ClientSecret);
                if (grantType == GrantType.AuthorizationCode)
                {
                    parameters.Add("code", refreshToken);
                }
                else
                {
                    parameters.Add("refresh_token", refreshToken);
                }
                var postResult = await _authClient.PostAsync(tokenQueryFormat, new FormUrlEncodedContent(parameters));
                var stringResult = await postResult.Content.ReadAsStringAsync();
                var token = await JsonConvert.DeserializeObjectAsync<Token>(stringResult);
                if (token != null)
                {
                    if (grantType == GrantType.RefreshToken)
                    {
                        token.RefreshToken = refreshToken;
                    }
                }
                _appSettings.SpotifyToken = token;
                if (token != null && token.TokenType != null && token.AccessToken != null)
                {
                    _client.DefaultRequestHeaders.Authorization
                        = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);
                    TimeSpan timeSpan = TimeSpan.FromSeconds(token.ExpiresIn - 30);
                    _timer.Change(timeSpan, timeSpan);
                    _log.Info("Token refreshed");
                }
                else
                {
                    _log.Info(stringResult);
                }
                
                if (TokenUpdated != null)
                {
                    TokenUpdated(this, null);
                }
            }
            catch (Exception ex)
            {
                _log.FatalException("Update token failed with" + ex.Message, ex);
            }
            
        }
    }
}
