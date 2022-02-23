using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mvis.Plugin.CloudMusicSupport.Misc
{
    public class APISearchResultInfo
    {
        [JsonProperty("songs")]
        public IList<APISongInfo> Songs { get; set; }

        [JsonProperty("songCount")]
        public int SongCount { get; set; }
    }
}
