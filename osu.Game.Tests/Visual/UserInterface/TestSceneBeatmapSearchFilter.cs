// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneBeatmapSearchFilter : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(BeatmapSearchFilter<>),
            typeof(BeatmapSearchRulesetFilter)
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private readonly FillFlowContainer resizableContainer;

        public TestSceneBeatmapSearchFilter()
        {
            Add(resizableContainer = new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 600,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    new BeatmapSearchRulesetFilter(),
                    new BeatmapSearchFilter<BeatmapSearchCategory>(),
                }
            });
        }

        [Test]
        public void TestResize()
        {
            AddStep("Resize to 100px", () => resizableContainer.ResizeWidthTo(100, 1000));
            AddStep("Resize to 600px", () => resizableContainer.ResizeWidthTo(600, 1000));
        }
    }
}
