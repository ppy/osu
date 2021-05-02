using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mvis.Plugin.CloudMusicSupport.Misc
{
    public class ResultInfo
    {
        [JsonProperty("songs")]
        public IList<SongInfo> Songs { get; set; }

        [JsonProperty("songCount")]
        public int SongCount { get; set; }
    }
}
