using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniPie.Core.SpotifyWeb;
using MiniPie.Core.SpotifyWeb.Models;

namespace MiniPie.Tests.Controller
{
    public class FakeSpotifyWebApi:ISpotifyWebApi
    {
        public event EventHandler TokenUpdated;
        public Task AddToPlaylist(string playlistId, string trackUrls)
        {
            throw new NotImplementedException();
        }

        public Uri BuildLoginQuery()
        {
            throw new NotImplementedException();
        }

        public Task CreateToken(string response)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetArt(string uri)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetProfile()
        {
            throw new NotImplementedException();
        }

        public Task<IList<Track>> GetTrackInfo(IList<string> trackIds)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Playlist>> GetUserPlaylists()
        {
            throw new NotImplementedException();
        }

        public Task Initialize()
        {
            throw new NotImplementedException();
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }

        public Task UpdateToken(string refreshToken, GrantType grantType = GrantType.AuthorizationCode)
        {
            throw new NotImplementedException();
        }
    }
}
