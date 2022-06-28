using osu.Framework.Graphics.Audio;

namespace osu.Game.Screens.LLin.Plugins.Types
{
    public interface IProvideAudioControlPlugin
    {
        /// <summary>
        /// 下一首
        /// </summary>
        /// <returns>操作是否被允许</returns>
        public bool NextTrack();

        /// <summary>
        /// 上一首
        /// </summary>
        /// <returns>操作是否被允许</returns>
        public bool PrevTrack();

        /// <summary>
        /// 切换暂停
        /// </summary>
        /// <returns>操作是否被允许</returns>
        public bool TogglePause();

        /// <summary>
        /// 调整歌曲进度到某一时间节点
        /// </summary>
        /// <param name="position">目标时间(毫秒)</param>
        /// <returns>操作是否被允许</returns>
        public bool Seek(double position);

        /// <summary>
        /// 获取当前音轨
        /// </summary>
        /// <returns>当前音轨</returns>
        public DrawableTrack GetCurrentTrack();

        /// <summary>
        /// 是否被选中为Mvis音频控制器
        /// </summary>
        bool IsCurrent { get; set; }
    }
}
