using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace MiniPie.Core.SpotifyLocal {
    public class SpotifyLocalApi : ISpotifyLocalApi
    {
        private readonly HttpClient _client;
        private readonly ILog _log;
        private readonly Uri _baseUri;
        private string _cfid;
        private string _oAuth;

        private const string playPositionJsonIndex = "playing_position\":";

        /// <summary>Initializes a new SpotifyAPI object which can be used to recieve</summary>
        public SpotifyLocalApi(ILog log, AppContracts contracts) {

            //emulate the embed code [NEEDED]
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Origin", "https://open.spotify.com");
            _client.DefaultRequestHeaders.Referrer = new Uri("https://open.spotify.com/?uri=spotify:track:5Zp4SWOpbuOdnsxLqwgutt");
            _client.DefaultRequestHeaders.Add("User-Agent", "MiniPie");
            _log = log;
            _baseUri = new Uri(contracts.SpotifyLocalHost);
        }

        public async Task Initialize()
        {
            await RenewToken();
        }

        public async Task RenewToken() {
            try {
                //Reset fields
                _oAuth = string.Empty;
                _cfid = string.Empty;

                _oAuth = await GetOAuth().ConfigureAwait(false);
                _cfid = (await GetCfid().ConfigureAwait(false)).token;
            }
            catch (Exception exc) {
                _log.WarnException("Failed to renew Spotify token", exc);
            }
        }

        public bool HasValidToken {
            get { return !string.IsNullOrEmpty(_oAuth) && !string.IsNullOrEmpty(_cfid); }
        }


        /// <summary>Gets the current Unix Timestamp. Mostly for internal use</summary>
        private int TimeStamp {
            get {
                return Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
            }
        }

        /// <summary>Gets the 'CFID', a unique identifier for the current session. Note: It's required to get the CFID before making any other calls.</summary>
        public async Task<Cfid> GetCfid()
        {
            var a = await SendLocalRequest("simplecsrf/token.json").ConfigureAwait(false);
            var d = JsonConvert.DeserializeObject<Cfid>(a);
            _cfid = d.token;
            return d;
        }

        string _uri = "";
        /// <summary>Used by SpotifyAPI.Play to play Spotify URI's. Change this URI and then call SpotifyAPI.Play</summary>
        public string Uri {
            get {
                return _uri;
            }
            set {
                _uri = value;
            }
        }

        /// <summary>Plays a certain URI and returns the status afterwards. Change SpotifyAPI.URI into the needed uri!</summary>
        public Task<Status> Play()
        {
            return SendLocalStatusRequest("remote/play.json?uri=" + Uri, true, true, -1);
        }

        /// <summary>Resume Spotify playback and return the status afterwards</summary>
        public Task<Status> Resume()
        {
            return SendLocalStatusRequest("remote/pause.json?pause=false", true, true, -1);
        }

        /// <summary>Pause Spotify playback and return the status afterwards</summary>
        public Task<Status> Pause()
        {
            return SendLocalStatusRequest("remote/pause.json?pause=true", true, true, -1);
        }

        public Task<Status> Queue(string url)
        {
            return SendLocalStatusRequest("remote/play.json?uri=" + url + "?action=queue", true, true, -1);
        } 

        public Task<Status> SendLocalStatusRequest(bool oauth, bool cfid, CancellationToken token, int wait = -1)
        {
            return SendLocalStatusRequest("remote/status.json", oauth, cfid, wait);
        }

        private void PopulateParameters(NameValueCollection query, 
            bool oauth, bool cfid, int wait = -1)
        {
            //This seems unnecessary
            //query["_"] = TimeStamp.ToString();

            if (oauth)
                query["oauth"] = _oAuth;
            if (cfid)
                query["csrf"] = _cfid;

            if (wait != -1)
            {
                query["returnafter"] = wait.ToString();
                query["returnon"] = "login,logout,play,pause,error,ap";
            }
        }
        
        /// <summary>Recieves a OAuth key from the Spotify site</summary>
        private async Task<string> GetOAuth()
        {
            var token =
                await
                    Helper.DeserializeStringAsync<Token>(
                        await _client.GetStringAsync("https://open.spotify.com/token").ConfigureAwait(false))
                        .ConfigureAwait(false);
            return token.TokenValue;
        }


        private Task<string> SendLocalRequest(string request) {
            return SendLocalRequest(request, false, false, -1);
        }

        private async Task<string> SendLocalRequest(string request, bool oauth, bool cfid, int wait)
        {
            var uriBuilder = new UriBuilder(new Uri(_baseUri, new Uri(request, UriKind.Relative)));
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            PopulateParameters(query, oauth, cfid, wait);

            uriBuilder.Query = query.ToString();
            var requestUri = uriBuilder.ToString();
            string response = null;
            try
            {
                response = await _client.GetStringAsync(requestUri).ConfigureAwait(false);
            }
            catch (Exception wExc)
            {
                _log.WarnException("SendLocalRequest failed", wExc);
                //perhaps spotifywebhelper isn't started (happens sometimes)
                if (Process.GetProcessesByName("SpotifyWebHelper").Length < 1)
                {
                    try
                    {
                        Process.Start(
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Spotify", "SpotifyWebHelper.exe"));
                        _log.Warn("Spotify web helper was stopped. Started web helper successfully");
                    }
                    catch (Exception exc)
                    {
                        _log.WarnException("Failed to start the Spotify webhelper", exc);
                    }
                    response = await _client.GetStringAsync(requestUri).ConfigureAwait(false);
                }
            }
            return response;
        }

        private async Task<Status> SendLocalStatusRequest(string request, bool oauth, bool cfid, int wait)
        {
            var stringResult = await SendLocalRequest(request, oauth, cfid, wait).ConfigureAwait(false);
            return await Helper.DeserializeStringAsync<Status>(stringResult ?? String.Empty);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
