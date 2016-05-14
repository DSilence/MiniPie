using System.Windows.Input;
using MiniPie.Core.Enums;

namespace MiniPie.Core.SpotifyNative.HotKeyManager
{
    public interface IKeyManager
    {
        bool CheckCombinationRegistered(Key key, KeyModifier modifier);
        void RegisterHotkey(HotKeyAction action, Key key, KeyModifier modifier);
        void UnregisterHotkey(HotKeyAction action);
    }
}