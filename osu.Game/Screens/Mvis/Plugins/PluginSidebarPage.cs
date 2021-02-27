using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis.SideBar;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.Plugins
{
    public class PluginSidebarPage : CompositeDrawable, ISidebarContent
    {
        private Container blockMouseContainer;

        private MvisPlugin plugin;

        public MvisPlugin Plugin
        {
            get => plugin;
            set
            {
                if (plugin != null) throw new InvalidOperationException("不能设置两次Plugin");

                plugin = value;

                value.Disabled.BindValueChanged(v =>
                {
                    switch (v.NewValue)
                    {
                        case true:
                            Schedule(() => blockMouseContainer.FadeIn(200));
                            break;

                        case false:
                            Schedule(() => blockMouseContainer.FadeOut(200));
                            break;
                    }
                }, true);
            }
        }

        public PluginSidebarPage(float resizeWidth, string title)
        {
            ResizeWidth = resizeWidth;
            Title = title;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(blockMouseContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Depth = float.MinValue,
                Children = new Drawable[]
                {
                    new FakeEditor.BlockMouseBox
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
            });
        }

        public float ResizeWidth { get; }
        public string Title { get; }
    }
}
