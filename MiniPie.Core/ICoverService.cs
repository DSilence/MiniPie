namespace MiniPie.Core {
    public interface ICoverService {
        string FetchCover(string artist, string track);
        double CacheSize();
        void ClearCache();
    }
}