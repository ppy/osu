using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Screens.LLin.Plugins.Config;

namespace Mvis.Plugin.CloudMusicSupport.Config
{
    public class LyricConfigManager : PluginConfigManager<LyricSettings>
    {
        public LyricConfigManager(Storage storage)
            : base(storage)
        {
        }

        /// <summary>
        /// 在这里初始化默认值, 更多用法请见 <see cref="ConfigManager"/>
        /// </summary>
        protected override void InitialiseDefaults()
        {
            SetDefault(LyricSettings.EnablePlugin, true);
            SetDefault(LyricSettings.LyricFadeInDuration, 200f, 0, 1000);
            SetDefault(LyricSettings.LyricFadeOutDuration, 200f, 0, 1000);
            SetDefault(LyricSettings.SaveLrcWhenFetchFinish, true);
            SetDefault(LyricSettings.NoExtraShadow, true);
            SetDefault(LyricSettings.AutoScrollToCurrent, false);
            SetDefault(LyricSettings.LyricDirection, Anchor.BottomCentre);
            SetDefault(LyricSettings.LyricPositionX, 0f, -1f, 1f);
            SetDefault(LyricSettings.LyricPositionY, 0f, -1f, 1f);
            base.InitialiseDefaults();
        }

        //配置文件名，已更改的值将在"plugin-{ConfigName}.ini"中保存
        protected override string ConfigName => "lyric";
    }

    public enum LyricSettings
    {
        EnablePlugin,
        LyricFadeInDuration,
        LyricFadeOutDuration,
        SaveLrcWhenFetchFinish,
        NoExtraShadow,
        AutoScrollToCurrent,
        LyricDirection,
        LyricPositionX,
        LyricPositionY
    }
}
