﻿using System;
using System.Windows;
using Caliburn.Micro;
using MiniPie.Core;
using MiniPie.Core.Enums;
using TinyIoC;
using ILog = MiniPie.Core.ILog;

namespace MiniPie.ViewModels {
    public sealed class ShellViewModel : Screen, IToggleVisibility {
        private readonly IWindowManager _WindowManager;
        private readonly ISpotifyController _SpotifyController;
        private readonly ICoverService _CoverService;
        private readonly IEventAggregator _EventAggregator;
        private readonly AppSettings _Settings;
        private readonly ILog _Logger;
        private const string NoCoverUri = @"pack://application:,,,/MiniPie;component/Images/LogoWhite.png";
        private const string UnknownCoverUri = @"pack://application:,,,/MiniPie;component/Images/LogoUnknown.png";

        private const string _songFriendlyNameFormat = "{0} - {1}";

        public event EventHandler<ToggleVisibilityEventArgs> ToggleVisibility;
        public event EventHandler CoverDisplayFadeOut;
        public event EventHandler CoverDisplayFadeIn;
        
        public ShellViewModel(IWindowManager windowManager, ISpotifyController spotifyController, ICoverService coverService, IEventAggregator eventAggregator, AppSettings settings, ILog logger) {
            _WindowManager = windowManager;
            _SpotifyController = spotifyController;
            _CoverService = coverService;
            _EventAggregator = eventAggregator;
            _Settings = settings;
            _Logger = logger;
            _ApplicationSize = _Settings.ApplicationSize;

            CoverImage = NoCoverUri;

            _SpotifyController.AttachTrackChangedHandler((o, e) => UpdateView());
            _SpotifyController.SpotifyOpened += (o, e) => SpotifyOpened();
            _SpotifyController.SpotifyExited += (o, e) => SpotifyExited();
            _SpotifyController.AttachTrackStatusChangedHandler(SpotifyControllerOnTrackStatusChanged);

            _Settings.PropertyChanged += (o, e) => {
                                             if (e.PropertyName == ApplicationSize.GetType().Name)
                                                 ApplicationSize = _Settings.ApplicationSize;
                                         };
        }

        protected override void OnViewLoaded(object view) {
            base.OnViewLoaded(view);

            if (!_SpotifyController.IsSpotifyInstalled())
                _WindowManager.ShowDialog(TinyIoCContainer.Current.Resolve<NoSpotifyViewModel>());

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

        private string _CurrentTrack;
        public string CurrentTrack {
            get { return _CurrentTrack; }
            set { _CurrentTrack = value; NotifyOfPropertyChange(() => CurrentTrack); }
        }

        private string _CurrentArtist;
        public string CurrentArtist {
            get { return _CurrentArtist; }
            set { _CurrentArtist = value; NotifyOfPropertyChange(() => CurrentArtist); }
        }

        private string _CoverImage;
        public string CoverImage {
            get { return _CoverImage; }
            set { _CoverImage = value; NotifyOfPropertyChange(() => CoverImage); }
        }

        private bool _CanPlayPause;
        public bool CanPlayPause {
            get { return _CanPlayPause; }
            set { _CanPlayPause = value; NotifyOfPropertyChange(() => CanPlayPause); }
        }

        private bool _CanPlayPrevious;
        public bool CanPlayPrevious {
            get { return _CanPlayPrevious; }
            set { _CanPlayPrevious = value; NotifyOfPropertyChange(() => CanPlayPrevious); }
        }

        private bool _canVolumeDown;
        public bool CanVolumeDown
        {
            get { return _canVolumeDown; }
            set { _canVolumeDown = value; NotifyOfPropertyChange(); }
        }

        private bool _canVolumeUp;
        public bool CanVolumeUp
        {
            get { return _canVolumeUp; }
            set { _canVolumeUp = value; NotifyOfPropertyChange(); }
        }

        private bool _CanPlayNext;
        public bool CanPlayNext {
            get { return _CanPlayNext; }
            set { _CanPlayNext = value; NotifyOfPropertyChange(() => CanPlayNext); }
        }

        private bool _HasTrackInformation;
        public bool HasTrackInformation {
            get { return _HasTrackInformation; }
            set { _HasTrackInformation = value; NotifyOfPropertyChange(() => HasTrackInformation); }
        }

        private ApplicationSize _ApplicationSize;
        public ApplicationSize ApplicationSize {
            get { return _ApplicationSize; }		    
            set { _ApplicationSize = value; NotifyOfPropertyChange(() => ApplicationSize); }
        }

        private bool _isPlaying;

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion

        public void ShowSettings() {
            _WindowManager.ShowDialog(TinyIoCContainer.Current.Resolve<SettingsViewModel>());
        }

        public void ShowAbout() {
            _WindowManager.ShowDialog(TinyIoCContainer.Current.Resolve<AboutViewModel>());
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

        public string GetTrackFriendlyName()
        {
            return String.Format(_songFriendlyNameFormat, CurrentArtist, CurrentTrack);
        }

        private double _maxProgress;
        public double MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value; 
                NotifyOfPropertyChange();
            }
        }

        private double _progress;
        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value; 
                NotifyOfPropertyChange();
            }
        }

        public void MinimizeMiniplayer(object sender, EventArgs e)
        {
            var window = Application.Current.MainWindow;
            window.Visibility = Visibility.Hidden;
        }

        public void MaximizeMiniplayer(object sender, EventArgs args)
        {
            var window = Application.Current.MainWindow;
            window.Visibility = Visibility.Visible;
            window.Activate();
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
                var track = _SpotifyController.GetSongName();
                var artist = _SpotifyController.GetArtistName();
                MaxProgress = status == null ? 0 : status.track.length;
                Progress = status == null ? 0 : status.playing_position;
                IsPlaying = status != null && status.playing;

                if (IsPlaying)
                    OnCoverDisplayFadeOut();

                HasTrackInformation = (!string.IsNullOrEmpty(track) || !string.IsNullOrEmpty(artist));
                CurrentTrack = string.IsNullOrEmpty(track) ? "-" : track;
                CurrentArtist = string.IsNullOrEmpty(artist) ? "-" : artist;

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
            }
            catch (Exception exc) {
                _Logger.FatalException("UpdateView() failed hard", exc);
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
    }
}