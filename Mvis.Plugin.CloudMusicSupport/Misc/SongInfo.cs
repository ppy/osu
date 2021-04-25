using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mvis.Plugin.CloudMusicSupport.Misc
{
    public class SongInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public IList<ArtistInfo> artists { get; set; }
        public AlbumInfo album { get; set; }
        public int duration { get; set; }
        public int copyrightId { get; set; }
        public short status { get; set; }
        public IList<string> alias { get; set; }
        public int rtype { get; set; }
        public int ftype { get; set; }
        public int mvid { get; set; }
        public int fee { get; set; }
        public string rUrl { get; set; }
        public ulong mark { get; set; }

        [JsonIgnore]
        public string ArtistsString
        {
            get
            {
                string result = "";

                foreach (var artist in artists)
                {
                    result += $"{artist.name}, ";
                }

                return result;
            }
        }
    }
}
