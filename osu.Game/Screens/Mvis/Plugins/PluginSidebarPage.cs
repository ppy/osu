using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis.Misc;
using osu.Game.Screens.Mvis.SideBar;
using osuTK;
using osuTK.Graphics;
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
                    Depth = float.MinValue,
                    Children = new Drawable[]
                    {
                        new BlockMouseBox
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(0.5f),
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = Color4.White.Opacity(0.6f),
                            Children = new Drawable[]
                            {
                                new SpriteIcon
                                {
                                    Icon = FontAwesome.Solid.Ban,
                                    Size = new Vector2(60),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                },
                                new OsuSpriteText
                                {
                                    Text = "插件不可用",
                                    Font = OsuFont.GetFont(size: 45, weight: FontWeight.Bold),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                }
                            }
                        },
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Plugin.Disabled.BindValueChanged(v =>
            {
                content.FadeTo(v.NewValue ? 0 : 1);
                placeholder.FadeTo(v.NewValue ? 1 : 0, 200);

                if (!v.NewValue && !contentInit)
                {
                    InitContent(Plugin);
                    contentInit = true;
                }
            }, true);
        }

        public float ResizeWidth { get; }
        public string Title { get; }
    }
}
