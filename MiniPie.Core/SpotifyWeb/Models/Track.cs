using System.Collections.Generic;

namespace MiniPie.Core.SpotifyWeb.Models
{
    [Equals]
    public class Track: ModelBase
    {
        public IList<Artist> Artists { get; set; }
        public string Name { get; set; }
    }
}
