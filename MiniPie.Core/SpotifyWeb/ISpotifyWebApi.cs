﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniPie.Core.SpotifyWeb.Models;

namespace MiniPie.Core.SpotifyWeb
{
    public interface ISpotifyWebApi
    {
        event EventHandler TokenUpdated;

        Task AddToPlaylist(string playlistId, string trackUrls);
        Uri BuildLoginQuery();
        Task CreateToken(string response);
        Task<string> GetArt(string uri);
        Task<User> GetProfile();
        Task<IList<Models.Track>> GetTrackInfo(IList<string> trackIds);
        Task<IList<Playlist>> GetUserPlaylists();
        Task Initialize();
        void Logout();
        Task UpdateToken(string refreshToken, GrantType grantType = GrantType.AuthorizationCode);

        Task<bool> IsTracksSaved(IList<string> trackIds);
    }
}