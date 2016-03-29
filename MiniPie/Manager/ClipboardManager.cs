using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
