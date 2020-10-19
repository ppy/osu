// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneBeatmapListingSortTabControl : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        public TestSceneBeatmapListingSortTabControl()
        {
            BeatmapListingSortTabControl control;
            OsuSpriteText current;
            OsuSpriteText direction;

            Add(control = new BeatmapListingSortTabControl
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            Add(new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5),
                Children = new Drawable[]
                {
                    current = new OsuSpriteText(),
                    direction = new OsuSpriteText()
                }
            });

            control.SortDirection.BindValueChanged(sortDirection => direction.Text = $"Sort direction: {sortDirection.NewValue}", true);
            control.Current.BindValueChanged(criteria => current.Text = $"Criteria: {criteria.NewValue}", true);
        }
    }
}
