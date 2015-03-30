using System;
using System.Windows;

namespace MiniPie {
    public sealed class ToggleVisibilityEventArgs : EventArgs {

        public ToggleVisibilityEventArgs(Visibility visibility) {
            Visibility = visibility;
        }

        public Visibility Visibility { get; private set; }
    }
}
