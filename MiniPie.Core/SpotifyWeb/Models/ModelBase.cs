using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MiniPie.Core.SpotifyWeb.Models
{
    [Equals]
    public abstract class ModelBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("href")]
        public string Href { get; set; }
        public ExternalUrl ExternalUrls { get; set; }
        public Followers Followers { get; set; }
        [JsonProperty("uri")]
        public string Uri { get; set; }
        public IList<Image> Images { get; set; }
    }
}
