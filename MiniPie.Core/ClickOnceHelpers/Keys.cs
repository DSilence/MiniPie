using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPie.Core.ClickOnceHelpers
{
    [Flags]
    public enum Keys
    {
        Shift = 65536,
        LButton = 1,
        Back = 8,
        Tab = Back | LButton,
        Space = 32,
        Down = Space | Back,
        MButton = 4,
        Clear = Back | MButton,
        Enter = Clear | Tab
    }
}
