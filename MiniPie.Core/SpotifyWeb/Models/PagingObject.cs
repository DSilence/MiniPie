using System.Collections.Generic;
using System.Linq;

namespace MiniPie.Core.SpotifyWeb.Models
{
    public class PagingObject<T>
    {
        public string Href { get; set; }
        public IList<T> Items { get; set; }
        public int Limit { get; set; }
        public string Next { get; set; }
        public int Offset { get; set; }
        public string Previous { get; set; }

        protected bool Equals(PagingObject<T> other)
        {
            return string.Equals(Href, other.Href) && Helper.NiceSequenceEqual(Items, other.Items) && Limit == other.Limit && string.Equals(Next, other.Next) && Offset == other.Offset && string.Equals(Previous, other.Previous) && Total == other.Total;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PagingObject<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Href != null ? Href.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Items != null ? Items.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Limit;
                hashCode = (hashCode*397) ^ (Next != null ? Next.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Offset;
                hashCode = (hashCode*397) ^ (Previous != null ? Previous.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Total;
                return hashCode;
            }
        }

        public int Total { get; set; }
    }
}
