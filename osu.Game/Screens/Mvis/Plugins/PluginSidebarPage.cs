using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.Placeholders;
using osu.Game.Screens.Mvis.Plugins.Config;
using osu.Game.Screens.Mvis.SideBar;
using osuTK.Input;

namespace osu.Game.Screens.Mvis.Plugins
{
    public abstract class PluginSidebarPage : Container, ISidebarContent
    {
        private readonly Container placeholder;
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

        protected PluginSidebarPage(MvisPlugin plugin, float resizeWidth)
        {
            ResizeWidth = resizeWidth;
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
                placeholder = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Children = new Drawable[]
                    {
                        bgBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre
                        },
                        new ClickablePlaceholder("请先启用该插件!", FontAwesome.Solid.Plug)
                        {
                            Action = () => pluginManager?.ActivePlugin(Plugin)
                        }
                    }
                }
            };
        }

        private DependencyContainer dependencies;
        private readonly Box bgBox;

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
                    placeholder.MoveToY(0, 500, Easing.OutQuint);
                }
                else
                {
                    content.FadeIn(200, Easing.OutQuint);
                    placeholder.MoveToY(1, 500, Easing.OutQuint);
                }

                if (!v.NewValue && !contentInit)
                {
                    InitContent(Plugin);
                    contentInit = true;
                }
            }, true);
        }

        protected override void LoadComplete()
        {
            colourProvider.HueColour.BindValueChanged(_ =>
            {
                bgBox.Colour = colourProvider.Background7;
            }, true);

            base.LoadComplete();
        }

        public float ResizeWidth { get; }
        public string Title { get; }
    }
}
