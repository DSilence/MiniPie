using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace MiniPie.ViewModels
{
    public class MiniPieScreen: Screen
    {
        public void OnPropertyChanged(string propertyName)
        {
            this.NotifyOfPropertyChange(propertyName);
        }
    }

    public class MiniPiePropertyChangedBase : PropertyChangedBase
    {
        public void OnPropertyChanged(string propertyName)
        {
            this.NotifyOfPropertyChange(propertyName);
        }
    }
}
