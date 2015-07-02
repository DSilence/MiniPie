using System.Threading.Tasks;

namespace MiniPie.Core {
    public interface ICoverService {
        Task<string> FetchCover(string artist, string track);
        double CacheSize();
        void ClearCache();
    }
}