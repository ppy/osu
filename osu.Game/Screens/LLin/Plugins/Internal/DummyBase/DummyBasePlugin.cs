using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Screens.LLin.Misc;
using osu.Game.Screens.LLin.Plugins.Config;
using osu.Game.Screens.LLin.Plugins.Types.SettingsItems;
using osu.Game.Screens.LLin.SideBar.Tabs;

namespace osu.Game.Screens.LLin.Plugins.Internal.DummyBase
{
    internal partial class DummyBasePlugin : LLinPlugin
    {
        internal DummyBasePlugin(MConfigManager config, LLinPluginManager plmgr)
        {
            HideFromPluginManagement = true;
            this.config = config;
            this.PluginManager = plmgr;

            Name = "基本设置";
            Version = LLinPluginManager.LatestPluginVersion;
        }

        private readonly MConfigManager config;

        public override SettingsEntry[] GetSettingEntries(IPluginConfigManager pluginConfigManager)
        {
            ListSettingsEntry<TypeWrapper> listEntry;
            var functionBarBindable = new Bindable<TypeWrapper>();

            var entries = new SettingsEntry[]
            {
                new NumberSettingsEntry<float>
                {
                    Name = "界面主题色(红)",
                    Bindable = config.GetBindable<float>(MSetting.MvisInterfaceRed),
                    KeyboardStep = 1
                },
                new NumberSettingsEntry<float>
                {
                    Name = "界面主题色(绿)",
                    Bindable = config.GetBindable<float>(MSetting.MvisInterfaceGreen),
                    KeyboardStep = 1
                },
                new NumberSettingsEntry<float>
                {
                    Name = "界面主题色(蓝)",
                    Bindable = config.GetBindable<float>(MSetting.MvisInterfaceBlue),
                    KeyboardStep = 1
                },
                new ColorPreviewEntry(),
                new NumberSettingsEntry<float>
                {
                    Name = "背景模糊",
                    Bindable = config.GetBindable<float>(MSetting.MvisBgBlur),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new NumberSettingsEntry<float>
                {
                    Name = "空闲时的背景亮度",
                    Bindable = config.GetBindable<float>(MSetting.MvisIdleBgDim),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new EnumSettingsEntry<TabControlPosition>
                {
                    Name = "TabControl位置",
                    Bindable = config.GetBindable<TabControlPosition>(MSetting.MvisTabControlPosition)
                },
                new BooleanSettingsEntry
                {
                    Name = "置顶Proxy",
                    Bindable = config.GetBindable<bool>(MSetting.MvisStoryboardProxy),
                    Description = "让所有Proxy显示在前景上方"
                },
                new BooleanSettingsEntry
                {
                    Name = "启用背景动画",
                    Bindable = config.GetBindable<bool>(MSetting.MvisEnableBgTriangles),
                    Description = "如果条件允许,播放器将会在背景显示动画"
                },
                listEntry = new ListSettingsEntry<TypeWrapper>
                {
                    Name = "底栏插件",
                    Bindable = functionBarBindable
                },
                new BooleanSettingsEntry
                {
                    Name = "自动启用垂直同步",
                    Bindable = config.GetBindable<bool>(MSetting.MvisAutoVSync),
                    Description = "启用后，将在进入播放器时自动启用垂直同步，并在退出时恢复帧数限制"
                },
                new NumberSettingsEntry<float>
                {
                    Name = "播放器设置最大宽度",
                    Bindable = config.GetBindable<float>(MSetting.MvisPlayerSettingsMaxWidth),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                    CommitOnMouseRelease = true
                },
            };

            var plugins = PluginManager.GetAllFunctionBarProviders();

            string currentFunctionBar = config.Get<string>(MSetting.MvisCurrentFunctionBar);

            foreach (var pl in plugins)
            {
                if (currentFunctionBar == PluginManager.ToPath(pl))
                {
                    functionBarBindable.Value = pl;
                }
            }

            listEntry.Values = plugins;
            functionBarBindable.Default = PluginManager.DefaultFunctionBarType;

            functionBarBindable.BindValueChanged(v =>
            {
                if (v.NewValue == null)
                {
                    config.SetValue(MSetting.MvisCurrentFunctionBar, string.Empty);
                    return;
                }

                var pl = v.NewValue;

                config.SetValue(MSetting.MvisCurrentFunctionBar, PluginManager.ToPath(pl));
            });

            return entries;
        }

        protected override Drawable CreateContent()
        {
            throw new System.NotImplementedException();
        }

        protected override bool OnContentLoaded(Drawable content)
        {
            throw new System.NotImplementedException();
        }

        protected override bool PostInit()
        {
            throw new System.NotImplementedException();
        }

        public override int Version { get; }

        private class ColorPreviewEntry : SettingsEntry
        {
            public override Drawable ToSettingsItem() => new ColourPreviewer();

            public override Drawable ToLLinSettingsItem() => null;
        }
    }
}
