using System;

namespace Mvis.Plugin.CloudMusicSupport.Misc.Mapping
{
    public class APIMappingRoot : IDisposable
    {
        /// <summary>
        /// 上次更新，每隔一周查询一次
        /// </summary>
        public int LastUpdate { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public APIBeatmapLyricMapping[] Data { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
