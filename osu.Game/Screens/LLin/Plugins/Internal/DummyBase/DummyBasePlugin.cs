using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Screens.LLin.Plugins.Types;
using osu.Game.Screens.LLin.Plugins.Types.SettingsItems;
using osu.Game.Screens.LLin.SideBar.Tabs;

namespace osu.Game.Screens.LLin.Plugins.Internal.DummyBase
{
    internal class DummyBasePlugin : LLinPlugin
    {
        internal DummyBasePlugin(MConfigManager config, LLinPluginManager plmgr)
        {
            HideFromPluginManagement = true;
            this.config = config;
            this.PluginManager = plmgr;

            Name = "基本设置";
            Version = LLinPluginManager.LatestPluginVersion;
        }

        private SettingsEntry[] entries;
        private readonly MConfigManager config;

        public override SettingsEntry[] GetSettingEntries()
        {
            if (entries == null)
            {
                ListSettingsEntry<IFunctionBarProvider> listEntry;
                var functionBarBindable = new Bindable<IFunctionBarProvider>();

                entries = new SettingsEntry[]
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
                    listEntry = new ListSettingsEntry<IFunctionBarProvider>
                    {
                        Name = "底栏插件",
                        Bindable = functionBarBindable
                    }
                };

                var plugins = PluginManager.GetAllFunctionBarProviders();
                plugins.Insert(0, PluginManager.DummyFunctionBar);

                string currentFunctionBar = config.Get<string>(MSetting.MvisCurrentFunctionBar);

                foreach (var pl in plugins)
                {
                    if (currentFunctionBar == PluginManager.ToPath(pl))
                    {
                        functionBarBindable.Value = pl;
                    }
                }

                listEntry.Values = plugins;
                functionBarBindable.BindValueChanged(v =>
                {
                    if (v.NewValue == null)
                    {
                        config.SetValue(MSetting.MvisCurrentFunctionBar, string.Empty);
                        return;
                    }

                    var pl = (LLinPlugin)v.NewValue;

                    config.SetValue(MSetting.MvisCurrentFunctionBar, PluginManager.ToPath(pl));
                });
            }

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
