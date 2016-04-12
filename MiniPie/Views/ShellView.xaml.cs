using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Hardcodet.Wpf.TaskbarNotification;
using MiniPie.ViewModels;
using Application = System.Windows.Application;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using FlowDirection = System.Windows.FlowDirection;
using Size = System.Windows.Size;
using UserControl = System.Windows.Controls.UserControl;

namespace MiniPie.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : UserControl
    {
        private readonly TaskbarIcon _notifyIcon;

        public ShellView()
        {
            InitializeComponent();
            _notifyIcon = (TaskbarIcon) this.Resources["NotifyIcon"];
        }

        private void MainWindowOnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            _notifyIcon.Visibility = Visibility.Collapsed;
            _notifyIcon.Dispose();
        }

        private void AlbumArt_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                var viewModel = ShellViewModel;
                if (viewModel != null)
                {
                    viewModel.OpenSpotifyWindow();
                }
            }
        }

        private ShellViewModel ShellViewModel
        {
            get { return DataContext as ShellViewModel; }
        }

        private void ShellView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Application.Current.MainWindow.Closing -= MainWindowOnClosing;
            Application.Current.MainWindow.Closing += MainWindowOnClosing;
            MiniPieContextMenu.DataContext = e.NewValue;
            _notifyIcon.ContextMenu.DataContext = e.NewValue;
        }


        private const int InitialBeginAnimationCyclePause = 1;
        private const int BeginAnimationCyclePause = 0;
        private const int EndAnimationCyclePause = 5;
        private double _speed;
        private readonly RepeatBehavior _once = new RepeatBehavior(1);

        private bool shouldAnimate;
        private bool _isStoryboardRunning;

        private void BeginStoryboard()
        {
            _isStoryboardRunning = true;
            Storyboard.Begin();
        }

        private void StopStoryboard()
        {
            if (_isStoryboardRunning)
            {
                _isStoryboardRunning = false;
                Storyboard.Stop();
            }
        }

        private void CurrentTrack_OnTargetUpdated(object sender, DataTransferEventArgs e)
        {
            CurrentTrack.IsMouseDirectlyOverChanged -= CurrentTrackOnIsMouseDirectlyOverChanged;
            var textBlock = CurrentTrack;
            var targetedWidth = MeasureString(textBlock.Text, textBlock).Width;

            var diff = targetedWidth - TitlePanel.ActualWidth + 7;
            if (textBlock.ActualWidth > 0 && diff > 0)
            {
                shouldAnimate = true;
                StopStoryboard();
                var animation = (DoubleAnimationUsingKeyFrames) Storyboard.Children.First();
                animation.RepeatBehavior = _once;
                MaxKeyFrame.Value = -diff;
                EndDelayKeyFrame.Value = -diff;
                _speed = diff/20;

                ModifyAnimation(animation, InitialBeginAnimationCyclePause);
                BeginStoryboard();
            }
            else
            {
                shouldAnimate = false;
                StopStoryboard();
            }
        }

        private void StoryboardOnCompleted(object sender, EventArgs eventArgs)
        {
            Debug.Print("StoryboardOnCompleted");
            CurrentTrack.IsMouseDirectlyOverChanged += CurrentTrackOnIsMouseDirectlyOverChanged;
        }

        private void CurrentTrackOnIsMouseDirectlyOverChanged(object sender,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            Debug.Print("CurrentTrackOnIsMouseDirectlyOverChanged");
            if (true.Equals(dependencyPropertyChangedEventArgs.NewValue) && shouldAnimate)
            {
                CurrentTrack.IsMouseDirectlyOverChanged -= CurrentTrackOnIsMouseDirectlyOverChanged;
                StopStoryboard();
                var animation = (DoubleAnimationUsingKeyFrames) Storyboard.Children.First();
                ModifyAnimation(animation, BeginAnimationCyclePause);
                BeginStoryboard();
            }
        }

        private void ModifyAnimation(DoubleAnimationUsingKeyFrames animation, int beginAnimationPause)
        {
            MinKeyTime.KeyTime = TimeSpan.FromSeconds(beginAnimationPause);
            double maxKeyTimeSeconds = _speed + beginAnimationPause;
            MaxKeyFrame.KeyTime = TimeSpan.FromSeconds(maxKeyTimeSeconds);
            TimeSpan max = TimeSpan.FromSeconds(maxKeyTimeSeconds + EndAnimationCyclePause);
            animation.Duration = max;
            EndDelayKeyFrame.KeyTime = max;
            ResetKeyFrame.KeyTime = max;
        }

        private Size MeasureString(string candidate, TextBlock target)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(target.FontFamily, target.FontStyle, target.FontWeight, target.FontStretch),
                target.FontSize, target.Foreground);

            return new Size(formattedText.Width, formattedText.Height);
        }

        private async void TitlePanel_OnDrop(object sender, DragEventArgs e)
        {
            var data = Convert.ToString(e.Data.GetData(DataFormats.Html));
            var viewModel = ShellViewModel;
            Clipboard.SetText(await Task.Run(() => viewModel.CopyTracksInfo(data)));
        }

        private async void AlbumArt_OnDrop(object sender, DragEventArgs e)
        {
            var data = Convert.ToString(e.Data.GetData(DataFormats.Text));
            var urls = data.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            //TODO url validation
            await ShellViewModel.AddTracksToQueue(urls);
        }

        private void NotifyIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ShellViewModel.HandleTrayMouseDoubleClick(Application.Current.MainWindow);
        }

        private void NotifyIcon_OnTrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            ShellViewModel.HandleTrayMouseClick(Application.Current.MainWindow);
        }
    }
}