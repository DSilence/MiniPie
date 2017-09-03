using System.Windows;

namespace MiniPie.Manager
{
    public class ClipboardManager
    {
        public virtual void SetText(string text)
        {
            Clipboard.SetText(text);
        }
    }
}
