using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Win32;
using MiniPie.Core;
using MiniPie.Core.Enums;
using MiniPie.Core.SpotifyWeb.Models;
using SimpleInjector;
using ILog = MiniPie.Core.ILog;

namespace MiniPie.ViewModels {
    public sealed class ShellViewModel : Screen, IToggleVisibility {
        private readonly IWindowManager _WindowManager;
        private readonly ISpotifyController _SpotifyController;
        private readonly ICoverService _CoverService;
        private readonly AppSettings _Settings;
        private readonly ILog _Logger;
        private readonly Container _kernel;
        private const string NoCoverUri = @"pack://application:,,,/MiniPie;component/Images/LogoWhite.png";
        private const string UnknownCoverUri = @"pack://application:,,,/MiniPie;component/Images/LogoUnknown.png";
        private const string TrackPattern = @".*\/\/open\.spotify\.com\/track\/(.*)";

        private const string _songFriendlyNameFormat = "{0} – {1}";

        public event EventHandler<ToggleVisibilityEventArgs> ToggleVisibility;
        public event EventHandler CoverDisplayFadeOut;
        public event EventHandler CoverDisplayFadeIn;
        private bool _isLockPaused;

        public ShellViewModel(IWindowManager windowManager, ISpotifyController spotifyController, 
            ICoverService coverService, AppSettings settings, ILog logger, Container kernel) {
            _WindowManager = windowManager;
            _SpotifyController = spotifyController;
            _CoverService = coverService;
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
            SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;
        }

        private void SystemEventsOnSessionSwitch(object sender,
            SessionSwitchEventArgs sessionSwitchEventArgs)
        {
            if (_Settings.LockScreenBehavior > 0)
            {
                if (sessionSwitchEventArgs.Reason == SessionSwitchReason.SessionLock)
                {
                    if (IsPlaying)
                    {
                        _SpotifyController.Pause();
                        _isLockPaused = true;
                    }
                }
                else if (sessionSwitchEventArgs.Reason == SessionSwitchReason.SessionUnlock)
                {
                    if ((_isLockPaused && !IsPlaying && _Settings.LockScreenBehavior == LockScreenBehavior.PauseUnpause)
                        || (!IsPlaying && _Settings.LockScreenBehavior == LockScreenBehavior.PauseUnpauseAlways))
                    {
                        _SpotifyController.Play();
                    }
                    _isLockPaused = false;
                }
            }
        }

        protected override void OnViewLoaded(object view) {
            base.OnViewLoaded(view);

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
            SystemEvents.SessionSwitch -= SystemEventsOnSessionSwitch;
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

        public bool Loading
        {
            get { return string.IsNullOrEmpty(CurrentTrack); }
        }

        #endregion

        public void ShowSettings() {
            _WindowManager.ShowDialog(_kernel.GetInstance<SettingsViewModel>());
        }

        public void ShowAbout() {
            _WindowManager.ShowDialog(_kernel.GetInstance<AboutViewModel>());
        }

        public async void PlayPause() {
            if (CanPlayPause)
            {
                await _SpotifyController.PausePlay().ConfigureAwait(false);
            }
        }

        public async void PlayPrevious() {
            if(CanPlayPrevious)
                await _SpotifyController.PreviousTrack().ConfigureAwait(false);
        }

        public async void PlayNext() {
            if(CanPlayNext)
                await _SpotifyController.NextTrack().ConfigureAwait(false);
        }

        public async void VolumeUp() {
            if(CanVolumeUp)
                await _SpotifyController.VolumeUp().ConfigureAwait(false);
        }

        public async void VolumeDown() {
            if(CanVolumeDown)
                await _SpotifyController.VolumeDown().ConfigureAwait(false);
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


        public void HandleTrayMouseDoubleClick()
        {
            var window = Application.Current.MainWindow;
            if (_Settings.SingleClickHide)
            {
                if (window.Visibility == Visibility.Hidden)
                {
                    ShowWindow(window);
                }
                else
                {
                    HideWindow(window);
                }
            }
            else
            {
                HideWindow(window);
            }
        }

        public void HandleTrayMouseClick()
        {
            var window = Application.Current.MainWindow;
            if (_Settings.SingleClickHide)
            {
                if (window.Visibility == Visibility.Hidden)
                {
                    ShowWindow(window);
                }
                else
                {
                    HideWindow(window);
                }
            }
            else
            {
                ShowWindow(window);
            }
        }

        private void ShowWindow(Window window)
        {
            window.Visibility = Visibility.Visible;
            window.Activate();
        }

        private void HideWindow(Window window)
        {
            window.Visibility = Visibility.Hidden;
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
                if (status.track != null)
                {
                    MaxProgress = status.track.length;
                    Progress = status.playing_position;
                    IsPlaying = status.playing;

                    if (IsPlaying)
                        OnCoverDisplayFadeOut();

                    HasTrackInformation = (!string.IsNullOrEmpty(track) || !string.IsNullOrEmpty(artist));
                    var currentTrack = string.IsNullOrEmpty(track) ? "-" : track;
                    var currentArtist = string.IsNullOrEmpty(artist) ? "-" : artist;
                    var trackFriendlyName = string.Format(_songFriendlyNameFormat, currentArtist, currentTrack);

                    TrackUrl = status.track.track_resource?.location.og;
                    SpotifyUri = status.track.track_resource?.uri;
                    CurrentTrack = currentTrack;
                    CurrentArtist = currentArtist;
                    TrackFriendlyName = trackFriendlyName;
                }
                TrackId = GetTrackId(TrackUrl);

                CanPlayPause = _SpotifyController.IsSpotifyOpen();
                CanPlayPrevious = _SpotifyController.IsSpotifyOpen();
                CanPlayNext = _SpotifyController.IsSpotifyOpen();
                CanVolumeDown = _SpotifyController.IsSpotifyOpen();
                CanVolumeUp = _SpotifyController.IsSpotifyOpen();

                if (_SpotifyController.IsSpotifyOpen() 
                    && !string.IsNullOrEmpty(track) 
                    && !string.IsNullOrEmpty(artist)) {
                    if(_Settings.DisableAnimations)
                        CoverImage = NoCoverUri; //Reset cover image, no cover is better than an old one
                    try
                    {
                        var coverUri = await _CoverService.FetchCover(status);
                        if (string.IsNullOrEmpty(coverUri))
                            coverUri = UnknownCoverUri;
                        CoverImage = coverUri;
                        if (IsPlaying)
                            OnCoverDisplayFadeIn();
                    }
                    catch (Exception e)
                    {
                        _Logger.WarnException("Failed to retrieve cover information with: " + e.Message, e);
                    }
                }
                else {
                    CoverImage = NoCoverUri;
                    if (IsPlaying)
                        OnCoverDisplayFadeIn();
                }

                var tokenPresent = _Settings.SpotifyToken != null;
                if (tokenPresent)
                {
                    UpdatePlaylists();
                    if (TrackId != null)
                    {
                        IsTrackSaved = (await _SpotifyController.IsTracksSaved(new[] {TrackId})).First();
                    }
                }
            }
            catch (Exception exc) {
                _Logger.FatalException("UpdateView() failed hard with: " + exc.Message, exc);
                _Logger.Fatal(exc.StackTrace);
            }
        }

        private async void UpdatePlaylists()
        {
            var newPlaylists = await _SpotifyController.GetPlaylists();
            if (Playlists == null ||!(await Task.Run(() => newPlaylists.SequenceEqual(Playlists))))
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
            if (trackUrl == null)
            {
                return null;
            }
            var match = Regex.Match(trackUrl, TrackPattern);
            if (match.Groups.Count > 0)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        public async Task<string> CopyTracksInfo(IList<string> trackUrls)
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
                return result;
                
            }
            catch (Exception e)
            {
                _Logger.WarnException("Failed to copy track info", e);
            }
            return null;
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
            IsTrackSaved = true;
            try
            {
                await _SpotifyController.AddToMyMusic(new[] {TrackId});
            }
            catch (Exception e)
            {
                IsTrackSaved = false;
                _Logger.WarnException("Failed to add track", e);
            }
        }

        public async void RemoveFromMyMusic()
        {
            IsTrackSaved = false;
            try
            {
                await _SpotifyController.RemoveFromMyMusic(new[] {TrackId});
            }
            catch (Exception e)
            {
                IsTrackSaved = true;
                _Logger.WarnException("Failed to remove track", e);
            }
        }
    }
}