// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneBeatmapListingSort : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(BeatmapListingSortTabControl),
            typeof(OverlaySortTabControl<>),
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private readonly FillFlowContainer placeholder;
        private readonly BeatmapListingSortTabControl control;

        public TestSceneBeatmapListingSort()
        {
            Add(control = new BeatmapListingSortTabControl
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            Add(placeholder = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5),
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            control.SortDirection.BindValueChanged(_ => updateBindablesVisual());
            control.Current.BindValueChanged(_ => updateBindablesVisual(), true);
        }

        private void updateBindablesVisual()
        {
            placeholder.Clear();

            placeholder.Add(new OsuSpriteText { Text = $"Current: {control.Current.Value}" });
            placeholder.Add(new OsuSpriteText { Text = $"Sort direction: {control.SortDirection.Value}" });
        }
    }
}
