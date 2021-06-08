using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Online.Placeholders;
using osu.Game.Screens.Mvis.Plugins.Config;
using osu.Game.Screens.Mvis.SideBar;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Mvis.Plugins
{
    public abstract class PluginSidebarPage : Container, ISidebarContent
    {
        private readonly ClickablePlaceholder placeholder;
        private readonly Container content;

        protected override Container<Drawable> Content => content;

        protected virtual void InitContent(MvisPlugin plugin)
        {
        }

        public virtual PluginBottomBarButton CreateBottomBarButton() => null;
        public virtual Key ShortcutKey => Key.Unknown;

        private bool contentInit;

        public MvisPlugin Plugin { get; }
        protected IPluginConfigManager Config => Dependencies.Get<MvisPluginManager>().GetConfigManager(Plugin);

        [Resolved]
        private MvisPluginManager pluginManager { get; set; }

        protected PluginSidebarPage(MvisPlugin plugin, float resizeWidth = -1)
        {
            Plugin = plugin;
            Title = plugin.Name;
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0
                },
                placeholder = new ClickablePlaceholder("请先启用该插件!", FontAwesome.Solid.Plug)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Action = () => pluginManager?.ActivePlugin(Plugin),
                    Scale = new Vector2(1.25f)
                }
            };

            if (resizeWidth != -1) Logger.Log("resizeWidth已废弃", level: LogLevel.Important);
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            dependencies.Cache(this);
            dependencies.Cache(Plugin);
            dependencies.Cache(Dependencies.Get<MvisPluginManager>().GetConfigManager(Plugin));

            Plugin.Disabled.BindValueChanged(v =>
            {
                if (v.NewValue)
                {
                    content.FadeOut();
                    placeholder.MoveToY(0, 200, Easing.OutQuint).FadeIn(200, Easing.OutQuint);
                }
                else
                {
                    content.FadeIn(200, Easing.OutQuint);
                    placeholder.MoveToY(30, 200, Easing.OutQuint).FadeOut(200, Easing.OutQuint);
                }

                if (!v.NewValue && !contentInit)
                {
                    InitContent(Plugin);
                    contentInit = true;
                }
            }, true);
        }

        public string Title { get; }
        public IconUsage Icon { get; set; } = FontAwesome.Solid.Plug;
    }
}
