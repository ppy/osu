using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Mvis.Plugin.CloudMusicSupport.Misc
{
    public class LyricResponseRoot
    {
        [JsonProperty("lrc")]
        public LyricInfo RawLyric;

        [JsonProperty("tlyric")]
        public LyricInfo RawTLyric;

        [CanBeNull]
        [JsonIgnore]
        public List<string> Lyrics => RawLyric?.RawLyric?.Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();

        [CanBeNull]
        [JsonIgnore]
        public List<string> Tlyrics => RawTLyric?.RawLyric?.Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}
