using System.Collections.Generic;

namespace MiniPie.Core.SpotifyWebApi
{
    public class Album
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IList<Image> Images { get; set; } 
    }
}
