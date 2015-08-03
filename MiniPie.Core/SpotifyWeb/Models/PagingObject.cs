using System.Collections.Generic;

namespace MiniPie.Core.SpotifyWeb.Models
{
    public abstract class PagingObject<T>
    {
        public string Href { get; set; }
        public IList<T> Items { get; set; }
        public int Limit { get; set; }
        public string Next { get; set; }
        public int Offset { get; set; }
        public string Previous { get; set; }
        public int Total { get; set; }
    }
}
