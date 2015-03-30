using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
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
            e.Handled = true;

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

            var modifier = e.KeyboardDevice.Modifiers;
            KeyModifier internalModifier = KeyModifier.None;
            switch (modifier)
            {
                case ModifierKeys.Control:
                {
                    internalModifier = KeyModifier.Ctrl;
                    break;
                }
                case ModifierKeys.Alt:
                {
                    internalModifier = KeyModifier.Alt;
                    break;
                }
                case ModifierKeys.Shift:
                {
                    internalModifier = KeyModifier.Shift;
                    break;
                }
                case ModifierKeys.Windows:
                {
                    internalModifier = KeyModifier.Win;
                    break;
                }
            }


            var viewModel = ViewModel;
            if (viewModel == null)
            {
                return;
            }

            KeyValuePair<Key, KeyModifier> newValue = new KeyValuePair<Key, KeyModifier>(key, internalModifier);
            if (ReferenceEquals(sender, PlayPause))
            {
                viewModel.HotKeyViewModel.PlayPause 
                    = newValue;
                if (viewModel.HotKeyViewModel.HotKeysEnabled)
                {
                    viewModel.HotKeyViewModel.PerformHotKeyUpdate();
                }
            }
            else if (ReferenceEquals(sender, Previous))
            {
                viewModel.HotKeyViewModel.Previous = newValue;
                if (viewModel.HotKeyViewModel.HotKeysEnabled)
                {
                    viewModel.HotKeyViewModel.PerformHotKeyUpdate();
                }
            }
            else if (ReferenceEquals(sender, Next))
            {
                viewModel.HotKeyViewModel.Next = newValue;
                if (viewModel.HotKeyViewModel.HotKeysEnabled)
                {
                    viewModel.HotKeyViewModel.PerformHotKeyUpdate();
                }
            }
            else if (ReferenceEquals(sender, VolumeDown))
            {
                viewModel.HotKeyViewModel.VolumeDown = newValue;
                if (viewModel.HotKeyViewModel.HotKeysEnabled)
                {
                    viewModel.HotKeyViewModel.PerformHotKeyUpdate();
                }
            }
            else if (ReferenceEquals(sender, VolumeUp))
            {
                viewModel.HotKeyViewModel.VolumeUp = newValue;
                if (viewModel.HotKeyViewModel.HotKeysEnabled)
                {
                    viewModel.HotKeyViewModel.PerformHotKeyUpdate();
                }
            }
        }

        private SettingsViewModel ViewModel
        {
            get { return DataContext as SettingsViewModel; }
        }

    }
}
