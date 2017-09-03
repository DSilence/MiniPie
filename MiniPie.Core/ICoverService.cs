using System.Threading.Tasks;
using MiniPie.Core.SpotifyLocal;
using System;

namespace MiniPie.Core {
    public interface ICoverService : IDisposable
    {
        Task<string> FetchCover(Status status);
        long CacheSize();
        void ClearCache();
    }
}