using System.Collections.Generic;

namespace MiniPie.Core.SpotifyWeb.Models
{
    public class Track: ModelBase
    {
        public IList<Artist> Artists { get; set; }
        public string Name { get; set; }

        protected bool Equals(Track other)
        {
            return base.Equals(other) && Equals(Artists, other.Artists) && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Track) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (Artists != null ? Artists.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
