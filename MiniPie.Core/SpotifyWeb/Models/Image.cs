﻿using Newtonsoft.Json;

namespace MiniPie.Core.SpotifyWeb.Models
{
    public class Image
    {
        [JsonProperty("height")]
        public int? Height { get; set; }
        [JsonProperty("width")]
        public int? Width { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
