using Newtonsoft.Json;

namespace Mvis.Plugin.CloudMusicSupport.Misc
{
    public class SongInfo
    {
        [JsonProperty("id")]
        public int ID { get; set; }
    }
}
