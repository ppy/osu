using Newtonsoft.Json;

namespace Mvis.Plugin.CloudMusicSupport.Misc
{
    public class LyricInfo
    {
        [JsonProperty("lyric")]
        public string RawLyric { get; set; }

        [JsonProperty("version")]
        public int Version;
    }
}
