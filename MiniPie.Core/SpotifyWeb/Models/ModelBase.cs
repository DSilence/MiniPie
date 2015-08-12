using System.Collections.Generic;

namespace MiniPie.Core.SpotifyWeb.Models
{
    public abstract class ModelBase
    {
        public string Id { get; set; }
        public string Href { get; set; }
        public ExternalUrl ExternalUrls { get; set; }
        public Followers Followers { get; set; }
        public string Url { get; set; }
        public IList<Image> Images { get; set; }

        protected bool Equals(ModelBase other)
        {
            return string.Equals(Id, other.Id) && string.Equals(Href, other.Href) &&
                   Equals(ExternalUrls, other.ExternalUrls) && Equals(Followers, other.Followers) &&
                   string.Equals(Url, other.Url) && Equals(Images, other.Images);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ModelBase) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Href != null ? Href.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ExternalUrls != null ? ExternalUrls.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Followers != null ? Followers.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Url != null ? Url.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Images != null ? Images.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
