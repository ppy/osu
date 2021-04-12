using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis.Misc;
using osu.Game.Screens.Mvis.SideBar;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.Plugins
{
    public abstract class PluginSidebarPage : Container, ISidebarContent
    {
        private readonly Container blockMouseContainer;
        private readonly Container content;

        protected override Container<Drawable> Content => content;

        protected virtual void InitContent(MvisPlugin plugin)
        {
        }

        public MvisPlugin Plugin { get; }

        protected PluginSidebarPage(MvisPlugin plugin, float resizeWidth)
        {
            ResizeWidth = resizeWidth;
            Plugin = plugin;
            Title = plugin.Name;

            InternalChildren = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0
                },
                blockMouseContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MinValue,
                    Children = new Drawable[]
                    {
                        new BlockMouseBox
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(0.5f),
                        },
                        new OsuSpriteText
                        {
                            Text = "插件已禁用",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InitContent(Plugin);
            Plugin.Disabled.BindValueChanged(v =>
            {
                content.FadeTo(v.NewValue ? 0 : 1);
                blockMouseContainer.FadeTo(v.NewValue ? 1 : 0, 200);
            }, true);
        }

        public float ResizeWidth { get; }
        public string Title { get; }
    }
}
