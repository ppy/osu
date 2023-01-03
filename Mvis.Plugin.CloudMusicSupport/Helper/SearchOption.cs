using System;
using Mvis.Plugin.CloudMusicSupport.Misc;
using osu.Game.Beatmaps;

namespace Mvis.Plugin.CloudMusicSupport.Helper
{
    public struct SearchOption
    {
        /// <summary>
        /// 和此SearchOption对应的<see cref="WorkingBeatmap"/>
        /// </summary>
        public WorkingBeatmap? Beatmap;

        public Action<APILyricResponseRoot>? OnFinish;
        public Action<string>? OnFail;

        /// <summary>
        /// 是否要不带艺术家搜索？
        /// </summary>
        public bool NoArtist;

        /// <summary>
        /// 失败后是否禁止重试？
        /// </summary>
        public bool NoRetry;

        /// <summary>
        /// 在发出请求前是否要尝试从本地缓存寻找和谱面ID对应的歌词文件？
        /// </summary>
        public bool NoLocalFile;

        /// <summary>
        /// 标题匹配阈值，值越高要求越严格
        /// </summary>
        public float TitleSimiliarThreshold;

        /// <summary>
        /// 通过给定的参数构建<see cref="SearchOption"/>.
        /// </summary>
        /// <param name="sourceBeatmap">目标<see cref="WorkingBeatmap"/>></param>
        /// <param name="noLocalFile"><see cref="NoLocalFile"/></param>
        /// <param name="onFinish">完成时要进行的动作</param>
        /// <param name="onFail">失败时要进行的动作</param>
        /// <param name="titleSimiliarThreshold"><see cref="TitleSimiliarThreshold"/></param>
        /// <returns>通过参数构建的<see cref="SearchOption"/>></returns>
        public static SearchOption From(WorkingBeatmap sourceBeatmap, bool noLocalFile,
                                        Action<APILyricResponseRoot>? onFinish, Action<string> onFail,
                                        float titleSimiliarThreshold)
        {
            return new SearchOption
            {
                Beatmap = sourceBeatmap,

                OnFinish = onFinish,
                OnFail = onFail,

                NoLocalFile = noLocalFile,

                TitleSimiliarThreshold = titleSimiliarThreshold
            };
        }

        /// <summary>
        /// 通过给定的<see cref="RequestFinishMeta"/>>构建<see cref="SearchOption"/>
        /// <br/>
        /// 构建时将自动设置<see cref="Beatmap"/>、<see cref="OnFinish"/>和<see cref="OnFail"/>，其他属性保持默认值
        /// </summary>
        /// <param name="requestFinishMeta">目标Meta</param>
        /// <returns>通过参数构建的<see cref="SearchOption"/>></returns>
        public static SearchOption FromRequestFinishMeta(RequestFinishMeta requestFinishMeta)
        {
            return new SearchOption
            {
                Beatmap = requestFinishMeta.SourceBeatmap,

                OnFinish = requestFinishMeta.OnFinish,
                OnFail = requestFinishMeta.OnFail,

                TitleSimiliarThreshold = requestFinishMeta.TitleSimiliarThreshold
            };
        }
    }
}
