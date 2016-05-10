using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPie.Controls.CircularProgressButton
{
    public interface IProgressReporter
    {
        double Maximum { get; set; }
        double Minimum { get; set; }
        double Value { get; set; }
    }
}
