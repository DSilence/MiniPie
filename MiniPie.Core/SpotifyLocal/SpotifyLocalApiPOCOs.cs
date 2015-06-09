using Newtonsoft.Json;

namespace MiniPie.Core.SpotifyLocal {
    public class ClientVersion {
        [JsonProperty("error")]
        public Error error { get; set; }

        [JsonProperty("version")]
        public int version { get; set; }

        [JsonProperty("client_version")]
        public string client_version { get; set; }

        public bool running { get; set; }
    }

    public class Cfid {
        public Error error { get; set; }

        public string token { get; set; }
    }

    public class Status {

        [JsonProperty("error")]
        public Error error { get; set; }

        [JsonProperty("version")]
        public int version { get; set; }

        [JsonProperty("client_version")]
        public string client_version { get; set; }

        [JsonProperty("playing")]
        public bool playing { get; set; }

        [JsonProperty("shuffle")]
        public bool shuffle { get; set; }

        [JsonProperty("repeat")]
        public bool repeat { get; set; }

        [JsonProperty("play_enabled")]
        public bool play_enabled { get; set; }

        [JsonProperty("prev_enabled")]
        public bool prev_enabled { get; set; }

        [JsonProperty("track")]
        public Track track { get; set; }

        [JsonProperty("playing_position")]
        public double playing_position { get; set; }

        [JsonProperty("server_time")]
        public int server_time { get; set; }

        [JsonProperty("volume")]
        public double volume { get; set; }

        [JsonProperty("online")]
        public bool online { get; set; }

        [JsonProperty("open_graph_state")]
        public OpenGraphState open_graph_state { get; set; }

        [JsonProperty("running")]
        public bool running { get; set; }
    }


    public class Error {
        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("message")]
        public string message { get; set; }
    }


    public class OpenGraphState {

        [JsonProperty("private_session")]
        public bool private_session { get; set; }

        [JsonProperty("posting_disabled")]
        public bool posting_disabled { get; set; }
    }

    public class Track {
        [JsonProperty("track_resource")]
        public Resource track_resource { get; set; }

        [JsonProperty("artist_resource")]
        public Resource artist_resource { get; set; }

        [JsonProperty("album_resource")]
        public Resource album_resource { get; set; }

        [JsonProperty("length")]
        public int length { get; set; }

        [JsonProperty("track_type")]
        public string track_type { get; set; }
    }

    public class Resource {

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("uri")]
        public string uri { get; set; }

        [JsonProperty("location")]
        public Location location { get; set; }
    }

    public class Location {
        [JsonProperty("og")]
        public string og { get; set; }
    }

}
