using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MiniPie.ViewModels;
using Application = System.Windows.Application;
using ContextMenu = System.Windows.Forms.ContextMenu;
using FlowDirection = System.Windows.FlowDirection;
using MenuItem = System.Windows.Forms.MenuItem;
using Message = Caliburn.Micro.Message;
using Size = System.Windows.Size;
using UserControl = System.Windows.Controls.UserControl;

namespace MiniPie.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : UserControl
    {
        private NotifyIcon _notifyIcon;
        private ContextMenu _menu;
        private MenuItem _songNameMenuItem;

        public ShellView()
        {
            InitializeComponent();

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Visible = true;
            _notifyIcon.Icon = Icon.ExtractAssociatedIcon(
                Assembly.GetExecutingAssembly().Location);
        }

        private void MainWindowOnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (_notifyIcon != null)
            {
                var viewModel = ShellViewModel;
                if (viewModel != null)
                {
                    _notifyIcon.MouseClick -= viewModel.MaximizeMiniplayer;
                    _notifyIcon.DoubleClick -= viewModel.MinimizeMiniplayer;
                }
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
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
            var oldContext = e.OldValue as ShellViewModel;
            if (oldContext != null)
            {
                if (_menu != null)
                {
                    foreach (MenuItem menuItem in _menu.MenuItems)
                    {
                        menuItem.Dispose();
                    }
                    _menu.Dispose();
                }
                _notifyIcon.ContextMenu = null;
                _notifyIcon.MouseClick -= oldContext.MaximizeMiniplayer;
                _notifyIcon.DoubleClick -= oldContext.MinimizeMiniplayer;
                oldContext.PropertyChanged += PropertyChangedForNotifyIcon;
            }

            var context = e.NewValue as ShellViewModel;
            if (context != null)
            {
                //TODO kinda bad
                Application.Current.MainWindow.Closing += MainWindowOnClosing;


                _notifyIcon.MouseClick += context.MaximizeMiniplayer;
                _notifyIcon.DoubleClick += context.MinimizeMiniplayer;

                List<MenuItem> menuItems = new List<MenuItem>(MiniPieContextMenu.Items.Count + 2);
                _songNameMenuItem = new MenuItem();
                _songNameMenuItem.Enabled = false;
                menuItems.Add(_songNameMenuItem);
                menuItems.Add(new MenuItem("-"));
                foreach (var item in MiniPieContextMenu.Items)
                {
                    var menuItem = item as System.Windows.Controls.MenuItem;
                    if (menuItem != null)
                    {
                        var attach = Message.GetAttach(menuItem);
                        var action = attach.Split('=')[1].Split(' ')[2].Trim(']');
                        Action delegateAction = (Action) Delegate.CreateDelegate(typeof (Action), context, action);
                        menuItems.Add(new MenuItem(menuItem.Header.ToString(), (o, args) =>
                        {
                            delegateAction();
                        }));
                    }
                    else
                    {
                        var separator = item as System.Windows.Controls.Separator;
                        if (separator != null)
                        {
                            menuItems.Add(new MenuItem("-"));
                        }
                    }
                }
                _notifyIcon.ContextMenu = _menu = new ContextMenu(menuItems.ToArray());
                _notifyIcon.Text
                    = TruncateWithEllipsis(ShellViewModel.GetTrackFriendlyName(), 63);
                context.PropertyChanged += PropertyChangedForNotifyIcon;
            }
        }

        private void PropertyChangedForNotifyIcon(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "CurrentTrack" ||
                propertyChangedEventArgs.PropertyName == "CurrentArtist")
            {
                string friendlyName = TruncateWithEllipsis(ShellViewModel.GetTrackFriendlyName(), 63);
                _notifyIcon.Text = friendlyName;
                _songNameMenuItem.Text = friendlyName;
            }
        }

        //Truncates a string to be no longer than a certain length
        public static string TruncateWithEllipsis(string s, int length)
        {
            //there may be a more appropiate unicode character for this
            const string Ellipsis = "...";

            if (Ellipsis.Length > length)
                throw new ArgumentOutOfRangeException("length", length, "length must be at least as long as ellipsis.");

            if (s.Length > length)
                return s.Substring(0, length - Ellipsis.Length) + Ellipsis;
            else
                return s;
        }


        private const int InitialBeginAnimationCyclePause = 1;
        private const int BeginAnimationCyclePause = 0;
        private const int EndAnimationCyclePause = 5;
        private double _speed;
        private readonly RepeatBehavior _once = new RepeatBehavior(1);

        private bool shouldAnimate;
        private void CurrentTrack_OnTargetUpdated(object sender, DataTransferEventArgs e)
        {
            CurrentTrack.IsMouseDirectlyOverChanged -= CurrentTrackOnIsMouseDirectlyOverChanged;
            var textBlock = CurrentTrack;
            var targetedWidth = MeasureString(textBlock.Text, textBlock).Width;

            var diff = targetedWidth - TitlePanel.ActualWidth + 7;
            if (textBlock.ActualWidth > 0 && diff > 0)
            {
                shouldAnimate = true;
                Storyboard.Stop();
                var animation = (DoubleAnimationUsingKeyFrames)Storyboard.Children.First();
                animation.RepeatBehavior = _once;
                MaxKeyFrame.Value = -diff;
                EndDelayKeyFrame.Value = -diff;
                _speed = diff/20;
                
                ModifyAnimation(animation, InitialBeginAnimationCyclePause);
                Storyboard.Begin();
            }
            else
            {
                shouldAnimate = false;
                Storyboard.Stop();
            }
        }

        private void StoryboardOnCompleted(object sender, EventArgs eventArgs)
        {
            Debug.Print("StoryboardOnCompleted");
            CurrentTrack.IsMouseDirectlyOverChanged += CurrentTrackOnIsMouseDirectlyOverChanged;
        }

        private void CurrentTrackOnIsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            Debug.Print("CurrentTrackOnIsMouseDirectlyOverChanged");
            if (true.Equals(dependencyPropertyChangedEventArgs.NewValue) && shouldAnimate)
            {
                CurrentTrack.IsMouseDirectlyOverChanged -= CurrentTrackOnIsMouseDirectlyOverChanged;
                Storyboard.Stop();
                var animation = (DoubleAnimationUsingKeyFrames)Storyboard.Children.First();
                ModifyAnimation(animation, BeginAnimationCyclePause);
                Storyboard.Begin();
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
    }
}
