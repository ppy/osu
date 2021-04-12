using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.SideBar.PluginsPage;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.SideBar
{
    public class SidebarPluginsPage : CompositeDrawable, ISidebarContent
    {
        public float ResizeWidth => 0.35f;
        public string Title => "插件";

        private MvisPluginManager manager;
        private FillFlowContainer<PluginPiece> flow;
        private FillFlowContainer placeholder;

        [BackgroundDependencyLoader]
        private void load(MvisPluginManager pluginManager)
        {
            RelativeSizeAxes = Axes.Both;

            manager = pluginManager;

            InternalChildren = new Drawable[]
            {
                placeholder = new FillFlowContainer
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
                            Icon = FontAwesome.Solid.Boxes,
                            Size = new Vector2(60),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        new OsuSpriteText
                        {
                            Text = "没有插件",
                            Font = OsuFont.GetFont(size: 45, weight: FontWeight.Bold),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = flow = new FillFlowContainer<PluginPiece>
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Spacing = new Vector2(10),
                        Padding = new MarginPadding(10)
                    }
                }
            };

            pluginManager.OnPluginAdd += addPiece;
            pluginManager.OnPluginUnLoad += removePiece;
        }

        protected override void LoadComplete()
        {
            foreach (var pl in manager.GetAllPlugins(false))
            {
                addPiece(pl);
            }

            base.LoadComplete();
        }

        private void addPiece(MvisPlugin plugin)
        {
            flow.Add(new PluginPiece(plugin));

            placeholder.FadeOut(300, Easing.OutQuint);
        }

        private void removePiece(MvisPlugin plugin)
        {
            foreach (var piece in flow)
            {
                if (piece.Plugin == plugin)
                {
                    flow.Remove(piece);
                    break;
                }
            }

            if (flow.Children.Count == 0) placeholder.FadeIn(300, Easing.OutQuint);
        }
    }
}
