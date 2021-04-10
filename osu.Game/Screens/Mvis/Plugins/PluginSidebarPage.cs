using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis.SideBar;

namespace osu.Game.Screens.Mvis.Plugins
{
    public class PluginSidebarPage : Container, ISidebarContent
    {
        private readonly Container blockMouseContainer;
        private readonly Container content;

        protected override Container<Drawable> Content => content;

        protected virtual void InitContent(MvisPlugin plugin)
        {
        }

        private MvisPlugin plugin;

        public MvisPlugin Plugin
        {
            get => plugin;
            set
            {
                plugin = value;

                InitContent(value);

                value.Disabled.BindValueChanged(v =>
                {
                    switch (v.NewValue)
                    {
                        case true:
                            Schedule(() =>
                            {
                                content.FadeOut();
                                blockMouseContainer.FadeIn(200);
                            });
                            break;

                        case false:
                            Schedule(() =>
                            {
                                content.FadeIn(200);
                                blockMouseContainer.FadeOut(200);
                            });
                            break;
                    }
                }, true);
            }
        }

        public PluginSidebarPage(float resizeWidth, string title)
        {
            ResizeWidth = resizeWidth;
            Title = title;

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
                        //new FakeEditor.BlockMouseBox
                        //{
                        //    RelativeSizeAxes = Axes.Both,
                        //    Colour = Color4.Black.Opacity(0.5f),
                        //},
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

        public float ResizeWidth { get; }
        public string Title { get; }
    }
}
