using System;
using osu.Framework.Extensions.EnumExtensions;

namespace osu.Game.Screens.LLin.Misc
{
    /// <summary>
    /// 播放器相关信息
    /// </summary>
    public class PlayerInfo
    {
        /// <summary>
        /// 播放器名称
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// 播放器版本
        /// </summary>
        public uint Version = 0;

        /// <summary>
        /// 播放器提供方/作者名
        /// </summary>
        public string VendorName = string.Empty;

        private PlayerFlags flags = PlayerFlags.None;

        /// <summary>
        /// 播放器支持的额外功能
        /// </summary>
        /// <exception cref="InvalidOperationException">设置了非法组合</exception>
        public PlayerFlags SupportedFlags
        {
            get => flags;
            set
            {
                //检查是否有非法组合
                if (value.HasFlagFast(PlayerFlags.None) && value != PlayerFlags.None)
                    throw new InvalidOperationException("PlayerFlags.None不能与其他PlayerFlags一起提供");

                flags = value;
            }
        }

        public override string ToString()
            => $"{VendorName} - {Name} ({Version} | {flags})";
    }

    [Flags]
    public enum PlayerFlags
    {
        None = 1, //bug: None = 0 时 HasFlagFast(PlayerFlags.None)始终为True
        OverlayProxy = 1 << 1,
        SidebarSupport = 1 << 2,

        All = OverlayProxy | SidebarSupport
    }
}
