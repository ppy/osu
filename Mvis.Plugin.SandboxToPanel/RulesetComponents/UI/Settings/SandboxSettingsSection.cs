using Mvis.Plugin.SandboxToPanel.RulesetComponents.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.UI.Settings
{
    public abstract partial class SandboxSettingsSection : Container
    {
        protected abstract string HeaderName { get; }

        protected override Container<Drawable> Content => content;

        private readonly OsuSpriteText header;
        private readonly LocalShadowContainer content;

        public SandboxSettingsSection()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    header = new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = HeaderName,
                        Font = OsuFont.GetFont(size: 30)
                    },
                    content = new LocalShadowContainer()
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            header.Colour = colours.Yellow;
        }

        private partial class LocalShadowContainer : InnerShadowContainer
        {
            protected override Container<Drawable> Content => content;

            private FillFlowContainer content;

            public LocalShadowContainer()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                CornerRadius.Value = 10;
                Depth.Value = 5;
            }

            protected override Container<Drawable> CreateContent() => new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding
                {
                    Vertical = 15,
                    Horizontal = 10
                },
                Child = content = new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5)
                }
            };
        }
    }
}
