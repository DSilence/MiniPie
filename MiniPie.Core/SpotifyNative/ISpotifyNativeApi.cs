using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MiniPie.Core.SpotifyNative
{
    public interface ISpotifyNativeApi
    {
        void NextTrack();
        void PreviousTrack();
        void OpenSpotify();
        Process SpotifyProcess { get; set; }

        Task AttachToProcess(Func<Process, Task> processFound, Action<Process> processExited);
    }
}