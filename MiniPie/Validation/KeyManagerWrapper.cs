using System.Windows;
using MiniPie.Core.SpotifyNative.HotKeyManager;

namespace MiniPie.Validation
{
    public class KeyManagerWrapper : DependencyObject
    {
        public KeyManager KeyManager
        {
            get { return (KeyManager)GetValue(KeyManagerProperty); }
            set { SetValue(KeyManagerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyManagerProperty =
            DependencyProperty.Register("KeyManager", typeof(KeyManager), typeof(KeyManagerWrapper), new PropertyMetadata());
    }
}