using Caliburn.Micro;
using Microsoft.Expression.Interactivity.Core;
using Action = System.Action;

namespace MiniPie.ViewModels
{
    public class PlaylistItemViewModel: PropertyChangedBase
    {
        public string Name { get; set; }
        public Action Action { get; set; }

        public ActionCommand Command => new ActionCommand(Action);

        public override string ToString()
        {
            return Name;
        }
    }
}
