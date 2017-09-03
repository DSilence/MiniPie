namespace MiniPie.Core.SpotifyWeb.Models
{
    public class Playlist: ModelBase
    {
        public bool Collaborative { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public User Owner { get; set; }
        public bool? Public { get; set; }
        public string SnapshotId { get; set; }
        public PagingObject<Track> Tracks { get; set; }
    }
}
