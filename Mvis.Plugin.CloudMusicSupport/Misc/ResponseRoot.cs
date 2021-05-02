using Newtonsoft.Json;

namespace Mvis.Plugin.CloudMusicSupport.Misc
{
    public class ResponseRoot
    {
        [JsonProperty("result")]
        public ResultInfo Result { get; set; }

        [JsonProperty("code")]
        public int ResponseCode { get; set; }
    }
}
