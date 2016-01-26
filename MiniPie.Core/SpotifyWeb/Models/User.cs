using Newtonsoft.Json;

namespace MiniPie.Core.SpotifyWeb.Models
{
    [Equals]
    public class User: ModelBase
    {
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
    }
}
