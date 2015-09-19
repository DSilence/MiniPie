using System.Collections.Generic;
using System.Linq;

namespace MiniPie.Core.SpotifyWeb.Models
{
    [Equals]
    public abstract class ModelBase
    {
        public string Id { get; set; }
        public string Href { get; set; }
        public ExternalUrl ExternalUrls { get; set; }
        public Followers Followers { get; set; }
        public string Url { get; set; }
        public IList<Image> Images { get; set; }
    }
}
