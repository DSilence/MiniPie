using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using MiniPie.Converter;
using MiniPie.Core.SpotifyNative.HotKeyManager;
using MiniPie.ViewModels;

namespace MiniPie.Views
{
    /// <summary>
    /// Interaction logic for HotKeyView.xaml
    /// </summary>
    public partial class HotKeyView : UserControl
    {
        public HotKeyView()
        {
            InitializeComponent();
        }

        private void HotKey_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            if (sender != null)
            {
                var key = e.Key;
                if (key == Key.System)
                {
                    key = e.SystemKey;
                }

                if (key == Key.LeftCtrl || key == Key.RightCtrl || key == Key.LeftAlt || key == Key.RightAlt ||
                    key == Key.LeftShift || key == Key.RightShift || key == Key.LWin || key == Key.RWin)
                {
                    return;
                }

                var modifiers = GetKeyModifiers();

                var viewModel = DataContext as HotKeyViewModel;
                if (viewModel == null)
                {
                    return;
                }

                var textBox = (TextBox)sender;
                var converter = (KeyCodeToReadableStringConverter)Resources["CodeConverter"];
                textBox.Text = (string)converter.Convert(new KeyValuePair<Key, KeyModifier>(key,
                    modifiers), typeof(string), null, null);
                var expression = textBox.GetBindingExpression(TextBox.TextProperty);
                expression?.UpdateSource();
            }
        }

        private KeyModifier GetKeyModifiers()
        {
            KeyModifier modifier = KeyModifier.None;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                modifier = modifier | KeyModifier.Shift;
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                modifier = modifier | KeyModifier.Ctrl;
            }
            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                modifier = modifier | KeyModifier.Alt;
            }
            if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
            {
                modifier = modifier | KeyModifier.Win;
            }
            return modifier;
        }
    }
}
