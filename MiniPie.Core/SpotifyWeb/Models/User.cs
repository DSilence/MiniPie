namespace MiniPie.Core.SpotifyWeb.Models
{
    public class User: ModelBase
    {
        protected bool Equals(User other)
        {
            return base.Equals(other) && string.Equals(DisplayName, other.DisplayName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((User) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
            }
        }

        public string DisplayName { get; set; }
    }
}
