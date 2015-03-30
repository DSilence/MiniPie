using System;

namespace MiniPie {
    public interface IToggleVisibility {
        event EventHandler<ToggleVisibilityEventArgs> ToggleVisibility;
    }
}
