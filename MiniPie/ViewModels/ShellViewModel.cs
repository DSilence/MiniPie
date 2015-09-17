using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Caliburn.Micro;
using MiniPie.Core;
using MiniPie.Core.Enums;
using MiniPie.Core.SpotifyWeb.Models;
using Ninject;
using ILog = MiniPie.Core.ILog;

namespace MiniPie.ViewModels {
    public sealed class ShellViewModel : Screen, IToggleVisibility {
        private readonly IWindowManager _WindowManager;
        private readonly ISpotifyController _SpotifyController;
        private readonly ICoverService _CoverService;
        private readonly IEventAggregator _EventAggregator;
        private readonly AppSettings _Settings;
        private readonly ILog _Logger;
        private readonly IKernel _kernel;
        private const string NoCoverUri = @"pack://application:,,,/MiniPie;component/Images/LogoWhite.png";
        private const string UnknownCoverUri = @"pack://application:,,,/MiniPie;component/Images/LogoUnknown.png";
        private const string TrackPattern = @".*\/\/open\.spotify\.com\/track\/(.*)";

        private const string _songFriendlyNameFormat = "{0} – {1}";

        public event EventHandler<ToggleVisibilityEventArgs> ToggleVisibility;
        public event EventHandler CoverDisplayFadeOut;
        public event EventHandler CoverDisplayFadeIn;

        public ShellViewModel(IWindowManager windowManager, ISpotifyController spotifyController, 
            ICoverService coverService, IEventAggregator eventAggregator, AppSettings settings, ILog logger, IKernel kernel) {
            _WindowManager = windowManager;
            _SpotifyController = spotifyController;
            _CoverService = coverService;
            _EventAggregator = eventAggregator;
            _Settings = settings;
            _Logger = logger;
            _kernel = kernel;
            ApplicationSize = _Settings.ApplicationSize;

            CoverImage = NoCoverUri;

            _SpotifyController.AttachTrackChangedHandler((o, e) => UpdateView());
            _SpotifyController.SpotifyOpened += (o, e) => SpotifyOpened();
            _SpotifyController.SpotifyExited += (o, e) => SpotifyExited();
            _SpotifyController.AttachTrackStatusChangedHandler(SpotifyControllerOnTrackStatusChanged);

            //TODO more app sizes
            ApplicationSize = ApplicationSize.Medium;
        }

        protected override void OnViewLoaded(object view) {
            base.OnViewLoaded(view);

            if (!_SpotifyController.IsSpotifyInstalled())
                _WindowManager.ShowDialog(_kernel.Get<NoSpotifyViewModel>());

            if (_Settings.StartMinimized)
            {
                OnToggleVisibility(new ToggleVisibilityEventArgs(Visibility.Hidden));
            }
            else
            {
                if (_Settings.HideIfSpotifyClosed && !_SpotifyController.IsSpotifyOpen())
                    OnToggleVisibility(new ToggleVisibilityEventArgs(Visibility.Hidden));
            }
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
        }

        #region Properties
        public string CurrentTrack { get; set; }
        public string CurrentArtist { get; set; }
        public string CoverImage { get; set; }
        public bool CanPlayPause { get; set; }
        public bool CanPlayPrevious { get; set; }
        public bool CanVolumeDown { get; set; }
        public bool CanVolumeUp { get; set; }
        public bool CanPlayNext { get; set; }
        public bool HasTrackInformation { get; set; }
        public ApplicationSize ApplicationSize { get; set; }
        public bool IsPlaying { get; set; }
        public ObservableCollection<Playlist> Playlists { get; set; }
        public double MaxProgress { get; set; }
        public double Progress { get; set; }
        public string TrackUrl { get; set; }
        public string SpotifyUri { get; set; }
        public string TrackFriendlyName { get; set; }
        public bool IsTrackSaved { get; set; }
        public string TrackId { get; set; }
        #endregion

        public void ShowSettings() {
            _WindowManager.ShowDialog(_kernel.Get<SettingsViewModel>());
        }

        public void ShowAbout() {
            _WindowManager.ShowDialog(_kernel.Get<AboutViewModel>());
        }

        public void PlayPause() {
            if (CanPlayPause)
            {
                _SpotifyController.PausePlay();
            }
        }

        public void PlayPrevious() {
            if(CanPlayPrevious)
                _SpotifyController.PreviousTrack();
        }

        public void PlayNext() {
            if(CanPlayNext)
                _SpotifyController.NextTrack();
        }

        public void VolumeUp() {
            if(CanVolumeUp)
                _SpotifyController.VolumeUp();
        }

        public void VolumeDown() {
            if(CanVolumeDown)
                _SpotifyController.VolumeDown();
        }

        public void OpenSpotifyWindow()
        {
            _SpotifyController.OpenSpotify();
        }

        public void CopyTrackName()
        {
            Clipboard.SetText(TrackFriendlyName);
        }

        public void CopySpotifyLink()
        {
            Clipboard.SetText(TrackUrl);
        }


        public void MinimizeMiniplayer()
        {
            var window = Application.Current.MainWindow;
            window.Visibility = Visibility.Hidden;
        }

        public void MaximizeMiniplayer()
        {
            var window = Application.Current.MainWindow;
            window.Visibility = Visibility.Visible;
            window.Activate();
        }

        internal void MinimizeMiniplayer(object sender, EventArgs e)
        {
            MinimizeMiniplayer();
        }

        internal void MaximizeMiniplayer(object sender, EventArgs args)
        {
            MaximizeMiniplayer();
        }

        public void Close()
        {
            this.TryClose();
        }

        private void SpotifyOpened() {
            if(_Settings.HideIfSpotifyClosed)
                OnToggleVisibility(new ToggleVisibilityEventArgs(Visibility.Visible));

            UpdateView();
        }

        private void SpotifyExited() {
            if(_Settings.HideIfSpotifyClosed)
                OnToggleVisibility(new ToggleVisibilityEventArgs(Visibility.Hidden));

            UpdateView();
        }

        private void SpotifyControllerOnTrackStatusChanged(object sender, EventArgs eventArgs)
        {
            var status = _SpotifyController.GetStatus();
            MaxProgress = status.track.length;
            Progress = status.playing_position;
            IsPlaying = status.playing;
        }

        private async void UpdateView() {
            try {
                var status = _SpotifyController.GetStatus();
                if (status == null)
                {
                    return;
                }

                var track = status.track?.track_resource?.name;
                var artist = status.track?.artist_resource?.name;
                MaxProgress = status.track.length;
                Progress = status.playing_position;
                IsPlaying = status.playing;

                if (IsPlaying)
                    OnCoverDisplayFadeOut();

                HasTrackInformation = (!string.IsNullOrEmpty(track) || !string.IsNullOrEmpty(artist));
                var currentTrack = string.IsNullOrEmpty(track) ? "-" : track;
                var currentArtist = string.IsNullOrEmpty(artist) ? "-" : artist;
                var trackFriendlyName = string.Format(_songFriendlyNameFormat, currentArtist, currentTrack);

                TrackUrl = status.track.track_resource.location.og;
                SpotifyUri = status.track.track_resource.uri;
                CurrentTrack = currentTrack;
                CurrentArtist = currentArtist;
                TrackFriendlyName = trackFriendlyName;
                TrackId = GetTrackId(TrackUrl);

                CanPlayPause = _SpotifyController.IsSpotifyOpen();
                CanPlayPrevious = _SpotifyController.IsSpotifyOpen();
                CanPlayNext = _SpotifyController.IsSpotifyOpen();
                CanVolumeDown = _SpotifyController.IsSpotifyOpen();
                CanVolumeUp = _SpotifyController.IsSpotifyOpen();

                if (_SpotifyController.IsSpotifyOpen() && !string.IsNullOrEmpty(track) && !string.IsNullOrEmpty(artist)) {
                    if(_Settings.DisableAnimations)
                        CoverImage = NoCoverUri; //Reset cover image, no cover is better than an old one

                    var coverUri = await _CoverService.FetchCover(status);
                    if (string.IsNullOrEmpty(coverUri))
                        coverUri = UnknownCoverUri;
                    CoverImage = coverUri;
                    if (IsPlaying)
                        OnCoverDisplayFadeIn();
                }
                else {
                    CoverImage = NoCoverUri;
                    if (IsPlaying)
                        OnCoverDisplayFadeIn();
                }

                UpdatePlaylists();
                IsTrackSaved = (await _SpotifyController.IsTracksSaved(new[] { TrackId})).First();
            }
            catch (Exception exc) {
                _Logger.FatalException("UpdateView() failed hard", exc);
            }
        }

        private async void UpdatePlaylists()
        {
            var newPlaylists = await _SpotifyController.GetPlaylists();
            if (Playlists == null ||!newPlaylists.SequenceEqual(Playlists))
            {
                Playlists = new ObservableCollection<Playlist>(newPlaylists);
            }
        }

        public async void AddToPlaylist(string id)
        {
            await _SpotifyController.AddToPlaylist(id, SpotifyUri);
        }

        private string GetTrackId(string trackUrl)
        {
            var match = Regex.Match(trackUrl, TrackPattern);
            if (match.Groups.Count > 0)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        public async void CopyTracksInfo(IList<string> trackUrls)
        {
            try
            {
                List<string> ids = new List<string>();
                foreach (var trackUrl in trackUrls)
                {
                    var id = GetTrackId(trackUrl);
                    if (id != null)
                    {
                        ids.Add(id);
                    }
                }
                var trackInfo = await _SpotifyController.GetTrackInfo(ids);
                var niceNames =
                    trackInfo.Select(
                        track =>
                            string.Format(_songFriendlyNameFormat,
                                track.Artists.First().Name, track.Name));
                var result = string.Join(Environment.NewLine, niceNames);
                Clipboard.SetText(result);
            }
            catch (Exception e)
            {
                _Logger.WarnException("Failed to copy track info", e);
            }
        }

        public async void AddTracksToQueue(IList<string> trackUrls)
        {
            try
            {
                await _SpotifyController.AddToQueue(trackUrls);
            }
            catch (Exception e)
            {
                _Logger.WarnException("Failed to add tracks to queue", e);
            }
        }
        private void OnToggleVisibility(ToggleVisibilityEventArgs e) {
            Execute.OnUIThread(() => {
                                   var handler = ToggleVisibility;
                                   if (handler != null) handler(this, e);
                               });
        }
        private void OnCoverDisplayFadeOut() {
            Execute.OnUIThread(() => {
                                   if (_Settings.DisableAnimations)
                                       return;

                                   var handler = CoverDisplayFadeOut;
                                   if (handler != null) handler(this, EventArgs.Empty);
                               });
        }

        private void OnCoverDisplayFadeIn() {
            Execute.OnUIThread(() => {
                                   if (_Settings.DisableAnimations)
                                       return;

                                   var handler = CoverDisplayFadeIn;
                                   if (handler != null) handler(this, EventArgs.Empty);
                               });
        }

        public async void AddToMyMusic()
        {
            await _SpotifyController.AddToMyMusic(new[] {TrackId});
            IsTrackSaved = true;
        }

        public async void RemoveFromMyMusic()
        {
            await _SpotifyController.RemoveFromMyMusic(new[] {TrackId});
            IsTrackSaved = false;
        }
    }
}