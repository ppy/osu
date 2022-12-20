using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Screens.LLin.SideBar.Settings.Sections;

namespace osu.Game.Screens.LLin.Plugins.Config
{
    [Obsolete("请使用GetSettingEntries")]
    public abstract partial class PluginSidebarSettingsSection : Section
    {
        private readonly LLinPlugin plugin;
        protected IPluginConfigManager ConfigManager;

        protected PluginSidebarSettingsSection(LLinPlugin plugin)
        {
            this.plugin = plugin;
            Title = plugin.Name;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            ConfigManager = dependencies.Get<LLinPluginManager>().GetConfigManager(plugin);
            return dependencies;
        }
    }

    [Cached]
    public partial class NewPluginSettingsSection : Section
    {
        private readonly LLinPlugin plugin;

        private readonly BindableFloat fillFlowMaxWidth = new BindableFloat();

        public NewPluginSettingsSection(LLinPlugin plugin)
        {
            this.plugin = plugin;
            Title = plugin.Name;

            Alpha = 0.02f;

            Name = $"{plugin}的section";
        }

        [BackgroundDependencyLoader]
        private void load(LLinPluginManager pluginManager, MConfigManager config)
        {
            config.BindWith(MSetting.MvisPlayerSettingsMaxWidth, fillFlowMaxWidth);

            foreach (var se in pluginManager.GetSettingsFor(plugin)!)
            {
                var item = se.ToLLinSettingsItem();
                if (item != null) Add(item);
            }
        }

        public int MaxRows { get; private set; } = 1;
        public Action<int> OnNewMaxRows;

        protected override void LoadComplete()
        {
            fillFlowMaxWidth.BindValueChanged(v => FadeoutThen(200, () =>
                {
                    //bug: 直接设置FillFlow的Width可能并不会生效，灵异事件？
                    Width = v.NewValue;

                    int nmr = (int)Math.Floor(DrawWidth / (150 + 10f));

                    if (nmr != MaxRows)
                    {
                        MaxRows = nmr;
                        OnNewMaxRows?.Invoke(MaxRows);
                    }

                    FillFlow.AutoSizeDuration = 200;
                    FillFlow.AutoSizeEasing = Easing.OutQuint;
                })
                , true);

            base.LoadComplete();
        }
    }
}
