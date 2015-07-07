using System.Threading.Tasks;
using MiniPie.Core.SpotifyLocal;

namespace MiniPie.Core {
    public interface ICoverService {
        Task<string> FetchCover(Status status);
        long CacheSize();
        void ClearCache();
    }
}