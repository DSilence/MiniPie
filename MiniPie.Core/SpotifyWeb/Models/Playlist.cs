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

        protected bool Equals(Playlist other)
        {
            return base.Equals(other) && Collaborative == other.Collaborative &&
                   string.Equals(Description, other.Description) && string.Equals(Name, other.Name) &&
                   Equals(Owner, other.Owner) && Public == other.Public && string.Equals(SnapshotId, other.SnapshotId) &&
                   Equals(Tracks, other.Tracks);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Playlist) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ Collaborative.GetHashCode();
                hashCode = (hashCode*397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Owner != null ? Owner.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Public.GetHashCode();
                hashCode = (hashCode*397) ^ (SnapshotId != null ? SnapshotId.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Tracks != null ? Tracks.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
