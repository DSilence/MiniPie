using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPie.Core.SpotifyWeb.Models
{
    [Equals]
    public class Artist : ModelBase
    {
        public string Name { get; set; }
    }
}
