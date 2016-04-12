using Newtonsoft.Json;

namespace MiniPie.Core.SpotifyWeb.Models
{
    [Equals]
    public class SearchResult
    {
        [JsonProperty("artists")]
        public PagingObject<Artist> Artists { get; set; }
        [JsonProperty("tracks")]
        public PagingObject<Track> Tracks { get; set; }
    }
}