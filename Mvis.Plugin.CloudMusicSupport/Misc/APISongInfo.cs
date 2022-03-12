using Newtonsoft.Json;

namespace Mvis.Plugin.CloudMusicSupport.Misc
{
    public class APISongInfo
    {
        [JsonProperty("id")]
        public int ID { get; set; }
    }
}
