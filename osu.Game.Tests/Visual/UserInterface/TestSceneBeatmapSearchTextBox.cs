// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneBeatmapSearchTextBox : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(BeatmapSearchTextBox)
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        public TestSceneBeatmapSearchTextBox()
        {
            Add(new BeatmapSearchTextBox
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 500,
            });
        }
    }
}
