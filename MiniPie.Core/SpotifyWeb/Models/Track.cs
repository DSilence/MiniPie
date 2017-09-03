using System.Collections.Generic;
using Newtonsoft.Json;

namespace MiniPie.Core.SpotifyWeb.Models
{
    public class Track: ModelBase
    {
        [JsonProperty("album")]
        public Album Album { get; set; }
        public IList<Artist> Artists { get; set; }
        public string Name { get; set; }
    }
}
