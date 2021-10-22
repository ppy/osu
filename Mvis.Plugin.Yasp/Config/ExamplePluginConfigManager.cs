using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Game.Screens.LLin.Plugins.Config;

namespace Mvis.Plugin.Yasp.Config
{
    public class YaspConfigManager : PluginConfigManager<YaspSettings>
    {
        public YaspConfigManager(Storage storage)
            : base(storage)
        {
        }

        /// <summary>
        /// 在这里初始化默认值, 更多用法请见 <see cref="ConfigManager"/>
        /// </summary>
        protected override void InitialiseDefaults()
        {
            SetDefault(YaspSettings.Scale, 1, 0, 5f);
            SetDefault(YaspSettings.EnablePlugin, true);
            base.InitialiseDefaults();
        }

        //配置文件名，已更改的值将在"plugin-{ConfigName}.ini"中保存
        protected override string ConfigName => "yap";
    }

    public enum YaspSettings
    {
        Scale,
        EnablePlugin
    }
}
