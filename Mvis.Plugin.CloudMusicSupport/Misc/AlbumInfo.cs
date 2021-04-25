using System.Collections.Generic;

namespace Mvis.Plugin.CloudMusicSupport.Misc
{
    public class AlbumInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public string picUrl { get; set; }
        public IList<string> alias { get; set; }
        public int albumSize { get; set; }
        public string img1v1Url { get; set; }
        public int img1v1 { get; set; }
        public string trans { get; set; }
        public ArtistInfo artist { get; set; }
    }
}