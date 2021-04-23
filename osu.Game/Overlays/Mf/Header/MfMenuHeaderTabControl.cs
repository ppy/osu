using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Overlays.Mf.Sections;

namespace osu.Game.Overlays.Mf.Header
{
    public class MfMenuHeaderTabControl : OverlayTabControl<MfMenuSection>
    {
        public const float HEIGHT = 47;
        private const float bar_height = 2;

        public MfMenuHeaderTabControl()
        {
            RelativeSizeAxes = Axes.X;

            Height = HEIGHT;
            BarHeight = bar_height;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            AccentColour = colourProvider.Highlight1;
        }

        protected override TabItem<MfMenuSection> CreateTabItem(MfMenuSection value) => new ProfileSectionTabItem(value)
        {
            AccentColour = AccentColour
        };

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.Y,
            AutoSizeAxes = Axes.X,
            Direction = FillDirection.Horizontal,
        };

        private class ProfileSectionTabItem : OverlayTabItem
        {
            public ProfileSectionTabItem(MfMenuSection value)
                : base(value)
            {
                Text.Text = value.Title;
                Text.Font = OsuFont.GetFont(size: 20);
                Text.Margin = new MarginPadding { Vertical = 15.5f }; // 15px padding + 1.5px line-height difference compensation
                Bar.ExpandedSize = 10;
                Bar.Margin = new MarginPadding { Bottom = bar_height };
            }
        }
    }
}
