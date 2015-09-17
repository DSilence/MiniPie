using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
    }
}
