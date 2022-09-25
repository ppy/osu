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
        public int[] Beatmaps { get; set; }

        /// <summary>
        /// 偏移
        /// </summary>
        public int Offset { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
