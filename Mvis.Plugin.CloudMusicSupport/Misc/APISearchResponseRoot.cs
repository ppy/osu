using Newtonsoft.Json;

namespace Mvis.Plugin.CloudMusicSupport.Misc
{
    public class APISearchResponseRoot
    {
        [JsonProperty("result")]
        public APISearchResultInfo Result { get; set; }

        [JsonProperty("code")]
        public int ResponseCode { get; set; }
    }
}
