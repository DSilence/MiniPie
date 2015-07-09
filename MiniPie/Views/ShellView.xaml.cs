using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using MiniPie.Controls;
using MiniPie.ViewModels;
using Message = Caliburn.Micro.Message;
using UserControl = System.Windows.Controls.UserControl;

namespace MiniPie.Views {
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : UserControl
    {
        private NotifyIcon _notifyIcon;
        private ContextMenu _menu;
        private MenuItem _songNameMenuItem;

        public ShellView() {
            InitializeComponent();

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Visible = true;
            _notifyIcon.Icon = Icon.ExtractAssociatedIcon(
                Assembly.GetExecutingAssembly().Location);
        }

        private void ShellView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_notifyIcon != null)
            {
                var viewModel = ShellViewModel;
                if (viewModel != null)
                {
                    _notifyIcon.MouseClick -= viewModel.MaximizeMiniplayer;
                    _notifyIcon.DoubleClick -= viewModel.MinimizeMiniplayer;
                }
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
            get
            {
                return DataContext as ShellViewModel;
            }
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
                    = ShellViewModel.GetTrackFriendlyName();
                context.PropertyChanged += PropertyChangedForNotifyIcon;
            }
        }

        private void PropertyChangedForNotifyIcon(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "CurrentTrack" || propertyChangedEventArgs.PropertyName == "CurrentArtist")
            {
                string friendlyName = ShellViewModel.GetTrackFriendlyName();
                _notifyIcon.Text = friendlyName;
                _songNameMenuItem.Text = friendlyName;
            }
        }
    }
}
