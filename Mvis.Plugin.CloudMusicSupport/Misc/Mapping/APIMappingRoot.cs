using System;

namespace Mvis.Plugin.CloudMusicSupport.Misc.Mapping
{
    public class APIMappingRoot : IDisposable
    {
        /// <summary>
        /// 上次更新，每隔一周查询一次
        /// -2代表此Root处于调试/本地模式，不要更新
        /// -1代表此Root需要每次启动都更新一次
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
