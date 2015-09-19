namespace MiniPie.Core.SpotifyWeb.Models
{
    public class Image
    {
        protected bool Equals(Image other)
        {
            return Height == other.Height && Width == other.Width && string.Equals(Url, other.Url);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Image) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Height.GetHashCode();
                hashCode = (hashCode*397) ^ Width.GetHashCode();
                hashCode = (hashCode*397) ^ (Url != null ? Url.GetHashCode() : 0);
                return hashCode;
            }
        }

        public int? Height { get; set; }
        public int? Width { get; set; }
        public string Url { get; set; }
    }
}
