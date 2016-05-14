using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MiniPie.Core.SpotifyWeb.Models;

namespace MiniPie.Core.SpotifyWeb
{
    public class SpotifyHttpClient : HttpClient
    {
        private const string TokenQueryFormat = "https://accounts.spotify.com/api/token";
        private readonly ILog _log;
        private readonly string _loginRedirectUrl;
        private readonly AppSettings _settings;
        private readonly Action<Token> _tokenUpdated;
        private readonly HttpClient _authClient;

        public SpotifyHttpClient(string baseUri, ILog log, string loginRedirectUrl, 
            AppSettings settings, Action<Token> tokenUpdated)
        {
            _authClient = new HttpClient();
            BaseAddress = new Uri(baseUri);
            _log = log;
            _loginRedirectUrl = loginRedirectUrl;
            _settings = settings;
            _tokenUpdated = tokenUpdated;
        }

        public SpotifyHttpClient(string baseUri, ILog log, string loginRedirectUrl, 
            AppSettings settings, Action<Token> tokenUpdated, HttpMessageHandler handler):base(handler)
        {
            _authClient = new HttpClient();
            BaseAddress = new Uri(baseUri);
            _log = log;
            _loginRedirectUrl = loginRedirectUrl;
            _settings = settings;
            _tokenUpdated = tokenUpdated;
        }

        public Task<T> DoGetAsync<T>(string url)
        {
            return DoGetAsync<T>(new Uri(url, UriKind.RelativeOrAbsolute));
        }

        public async Task<T> DoGetAsync<T>(Uri uri)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                using (var result = await ExecuteUnwrapping(request).ConfigureAwait(false))
                {
                    var resultOutput = await result.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    var resultObject = await Helper.DeserializeStreamAsync<T>(resultOutput).ConfigureAwait(false);
                    return resultObject;
                }
            }
        }

        public async Task DoDeleteAsync(Uri uri)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, uri))
            {
                using (var result = await ExecuteUnwrapping(request).ConfigureAwait(false))
                {
                    //Do nothing
                }
            }
        }

        public Task DoDeleteAsync(string uri)
        {
            return DoDeleteAsync(new Uri(uri, UriKind.RelativeOrAbsolute));
        }

        public async Task DoPostAsync(Uri uri, object body)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                await PrepareRequest(request, body).ConfigureAwait(false);
                using (await ExecuteUnwrapping(request).ConfigureAwait(false)) { }
            }
        }

        public async Task DoPutAsync(Uri uri, object body)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, uri))
            {
                await PrepareRequest(request, body).ConfigureAwait(false);
                using (var responseMessage = await ExecuteUnwrapping(request).ConfigureAwait(false))
                {
                    //Do nothing - success
                }
            }
        }

        public Task DoPutAsync(string uri, object body = null)
        {
            return DoPutAsync(new Uri(uri, UriKind.RelativeOrAbsolute), body);
        }

        public Task DoPostAsync(string uri, object body = null)
        {
            return DoPostAsync(new Uri(uri, UriKind.RelativeOrAbsolute), body);
        }

        public async Task PrepareRequest(HttpRequestMessage request, object body = null)
        {
            if (body != null)
            {
                var stream = new MemoryStream();
                await Helper.SerializeToStreamAsync(body, stream).ConfigureAwait(false);
                stream.Position = 0;
                request.Content = new StreamContent(stream);
            }
        }

        public async Task<HttpResponseMessage> ExecuteUnwrapping(HttpRequestMessage request)
        {
            HttpResponseMessage result = null;
            try
            {
                result = await SendAsync(request).ConfigureAwait(false);
                if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (_settings.SpotifyToken != null)
                    {
                        //reauthenticate and try again
                        await Authenticate(_settings.SpotifyToken.RefreshToken, GrantType.RefreshToken)
                            .ConfigureAwait(false);

                        result.Dispose();
                        using (var messageCopy = CreateRequestCopy(request))
                        {
                            result = await SendAsync(messageCopy).ConfigureAwait(false);
                        }
                    }
                }
                result.EnsureSuccessStatusCode();
                return result;
            }
            catch (WebException webException)
            {
                if (webException.Response != null && ((HttpWebResponse)webException.Response).StatusCode != HttpStatusCode.NotFound)
                    _log.WarnException("[WebException] Failed to send request to spotify web api:" + request.RequestUri + "; the error is:" + webException.Message, webException);
                throw;
            }
            catch (Exception exc)
            {
                _log.WarnException("Failed to process Spotify request:" + request.RequestUri + "; the error is:" + exc.Message, exc);
                throw;
            }
        }

        private HttpRequestMessage CreateRequestCopy(HttpRequestMessage message)
        {
            HttpRequestMessage copy = new HttpRequestMessage(message.Method, message.RequestUri);
            copy.Content = message.Content;
            return copy;
        }

        public async Task Authenticate(string refreshToken,
            GrantType grantType = GrantType.AuthorizationCode)
        {
            try
            {
                _log.Info("Refreshing token");
                _log.Info("Token type:" + grantType.GetDescription());
                if (refreshToken == null)
                {
                    return;
                }
                var parameters = new Dictionary<string, string>
                {
                    {"grant_type", grantType.GetDescription()},
                    {"redirect_uri", _loginRedirectUrl},
                    {"client_id", AppContracts.ClientId},
                    {"client_secret", AppContracts.ClientSecret}
                };
                if (grantType == GrantType.AuthorizationCode)
                {
                    parameters.Add("code", refreshToken);
                }
                else
                {
                    parameters.Add("refresh_token", refreshToken);
                }
                var postResult =
                    await _authClient.PostAsync(TokenQueryFormat, new FormUrlEncodedContent(parameters)).ConfigureAwait(false);
                var stringResult = await postResult.Content.ReadAsStringAsync().ConfigureAwait(false);
                var token = await Helper.DeserializeStringAsync<Token>(stringResult).ConfigureAwait(false);
                if (token != null)
                {
                    if (grantType == GrantType.RefreshToken)
                    {
                        token.RefreshToken = refreshToken;
                    }
                }
                _settings.SpotifyToken = token;
                if (token != null && token.TokenType != null && token.AccessToken != null)
                {
                    this.DefaultRequestHeaders.Authorization
                        = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);
                    _log.Info("Token refreshed");
                }
                else
                {
                    _log.Info(stringResult);
                }

                _tokenUpdated?.Invoke(token);
            }
            catch (Exception ex)
            {
                _log.FatalException("Update token failed with" + ex.Message, ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _authClient.Dispose();
            base.Dispose(disposing);
        }
    }
}