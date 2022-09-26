using System;
using Newtonsoft.Json;

namespace Mvis.Plugin.CloudMusicSupport.Misc.Mapping
{
    public class APIBeatmapLyricMapping : IDisposable
    {
        /// <summary>
        /// 目标歌曲的网易云ID
        /// </summary>
        [JsonProperty("Target")]
        public int TargetNeteaseID { get; set; }

        /// <summary>
        /// 对应的谱面
        /// </summary>
        public int[] Beatmaps { get; set; } = Array.Empty<int>();

        /// <summary>
        /// 要匹配的标题
        /// </summary>
        public string[] MatchingTitle { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 匹配模式
        /// </summary>
        public MatchingMode TitleMatchMode = MatchingMode.Exactly;

        /// <summary>
        /// 要匹配的艺术家
        /// </summary>
        public string[] MatchingArtist { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 艺术家匹配模式
        /// </summary>
        public MatchingMode ArtistMatchMode = MatchingMode.Exactly;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    public enum MatchingMode
    {
        Exactly,
        Contains,
    }
}
