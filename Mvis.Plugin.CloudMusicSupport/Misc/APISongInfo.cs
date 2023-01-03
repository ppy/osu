using Newtonsoft.Json;

namespace Mvis.Plugin.CloudMusicSupport.Misc
{
    public class APISongInfo
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }
}
