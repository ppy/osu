using System.ComponentModel;
using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Game.Screens.LLin.Plugins.Config;

namespace Mvis.Plugin.Example.Config
{
    public class ExamplePluginConfigManager : PluginConfigManager<ExamplePluginSettings>
    {
        public ExamplePluginConfigManager(Storage storage)
            : base(storage)
        {
        }

        /// <summary>
        /// 在这里初始化默认值, 更多用法请见 <see cref="ConfigManager"/>
        /// </summary>
        protected override void InitialiseDefaults()
        {
            SetDefault(ExamplePluginSettings.KeyString, "Value1");
            SetDefault(ExamplePluginSettings.KeyFloat, 1, 0, 1f);
            SetDefault(ExamplePluginSettings.KeyDouble, 1, 0, 1d);
            SetDefault(ExamplePluginSettings.KeyBool, true);
            SetDefault(ExamplePluginSettings.keyEnum, ExampleEnum.Key1);
            base.InitialiseDefaults();
        }

        //配置文件名，已更改的值将在"plugin-{ConfigName}.ini"中保存
        protected override string ConfigName => "example";
    }

    public enum ExamplePluginSettings
    {
        KeyString,
        KeyFloat,
        KeyDouble,
        KeyBool,
        keyEnum
    }

    public enum ExampleEnum
    {
        [Description("我是Key1的描述")]
        Key1,

        Key2,
    }
}
