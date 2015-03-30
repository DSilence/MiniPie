using System.Windows;
using Caliburn.Micro;
using MiniPie.Core;

namespace MiniPie.ViewModels {
    public sealed class NoSpotifyViewModel : Screen, IFixedPosition {
        private readonly AppContracts _Contracts;

        public NoSpotifyViewModel(AppContracts contracts) {
            _Contracts = contracts;
            DisplayName = string.Format("No Spotify installed - {0}", _Contracts.ApplicationName);
        }

        public void Close() {
            TryClose();
        }

        public void GoToSpotify() {
            Helper.OpenUrl(_Contracts.SpotifyUrl);
        }

        public WindowStartupLocation WindowStartupLocation {
            get { return WindowStartupLocation.CenterScreen; }
        }
    }
}
