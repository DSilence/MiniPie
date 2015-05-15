using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using MiniPie.Core.Enums;
using MiniPie.Core.HotKeyManager;
using MiniPie.ViewModels;

namespace MiniPie.Views {
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl {
        public SettingsView() {
            InitializeComponent();
        }

        private void HotKey_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            //TODO the processing should be moved to view model
            e.Handled = true;

            if (sender != null)
            {
                var key = e.Key;

                if (key == Key.LeftCtrl || key == Key.RightCtrl || key == Key.LeftAlt || key == Key.RightAlt ||
                    key == Key.LeftShift || key == Key.RightShift || key == Key.LWin || key == Key.RWin)
                {
                    return;
                }

                if (key == Key.System)
                {
                    key = e.SystemKey;
                }

                var viewModel = ViewModel;
                if (viewModel == null)
                {
                    return;
                }

                if (ReferenceEquals(sender, PlayPause))
                {
                    KeyValuePair<Key, KeyModifier> newValue = 
                        new KeyValuePair<Key, KeyModifier>(key, 
                            viewModel.HotKeyViewModel.PlayPause.Value);
                    viewModel.HotKeyViewModel.PlayPause
                        = newValue;
                    if (viewModel.HotKeyViewModel.HotKeysEnabled)
                    {
                        viewModel.HotKeyViewModel.PerformHotKeyUpdate();
                    }
                }
                else if (ReferenceEquals(sender, Previous))
                {
                    KeyValuePair<Key, KeyModifier> newValue =
                        new KeyValuePair<Key, KeyModifier>(key,
                            viewModel.HotKeyViewModel.Previous.Value);
                    viewModel.HotKeyViewModel.Previous = newValue;
                    if (viewModel.HotKeyViewModel.HotKeysEnabled)
                    {
                        viewModel.HotKeyViewModel.PerformHotKeyUpdate();
                    }
                }
                else if (ReferenceEquals(sender, Next))
                {
                    KeyValuePair<Key, KeyModifier> newValue =
                        new KeyValuePair<Key, KeyModifier>(key,
                            viewModel.HotKeyViewModel.Next.Value);
                    viewModel.HotKeyViewModel.Next = newValue;
                    if (viewModel.HotKeyViewModel.HotKeysEnabled)
                    {
                        viewModel.HotKeyViewModel.PerformHotKeyUpdate();
                    }
                }
                else if (ReferenceEquals(sender, VolumeDown))
                {
                    KeyValuePair<Key, KeyModifier> newValue =
                       new KeyValuePair<Key, KeyModifier>(key,
                           viewModel.HotKeyViewModel.VolumeDown.Value);
                    viewModel.HotKeyViewModel.VolumeDown = newValue;
                    if (viewModel.HotKeyViewModel.HotKeysEnabled)
                    {
                        viewModel.HotKeyViewModel.PerformHotKeyUpdate();
                    }
                }
                else if (ReferenceEquals(sender, VolumeUp))
                {
                    KeyValuePair<Key, KeyModifier> newValue =
                      new KeyValuePair<Key, KeyModifier>(key,
                          viewModel.HotKeyViewModel.VolumeUp.Value);
                    viewModel.HotKeyViewModel.VolumeUp = newValue;
                    if (viewModel.HotKeyViewModel.HotKeysEnabled)
                    {
                        viewModel.HotKeyViewModel.PerformHotKeyUpdate();
                    }
                }
            }
        }
        private SettingsViewModel ViewModel
        {
            get { return DataContext as SettingsViewModel; }
        }
    }
}
