using System.Threading.Tasks;

namespace MiniPie.Core.SpotifyLocal
{
    public interface ISpotifyLocalApi
    {
        bool HasValidToken { get; }
        string Uri { get; set; }
        int Wait { get; set; }

        Task<Cfid> GetCfid();
        Task Initialize();
        Task<Status> Pause();
        Task<Status> Play();
        Task<Status> Queue(string url);
        Task RenewToken();
        Task<Status> Resume();
        Task<Status> SendLocalStatusRequest(bool oauth, bool cfid, int wait = -1);
    }
}