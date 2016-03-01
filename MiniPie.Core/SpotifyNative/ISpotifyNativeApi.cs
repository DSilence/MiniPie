using System.Diagnostics;
using System.Threading.Tasks;

namespace MiniPie.Core.SpotifyNative
{
    public interface ISpotifyNativeApi
    {
        void NextTrack();
        void PreviousTrack();
        void VolumeUp();
        void VolumeDown();
        void OpenSpotify();
        Process SpotifyProcess { get; set; }
    }
}