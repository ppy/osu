// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Overlays;
using NUnit.Framework;
using osu.Game.Overlays.BeatmapListing;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneBeatmapListingOverlay : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(BeatmapListingOverlay),
            typeof(BeatmapListingFilterControl)
        };

        protected override bool UseOnlineAPI => true;

        private readonly BeatmapListingOverlay overlay;

        public TestSceneBeatmapListingOverlay()
        {
            Add(overlay = new BeatmapListingOverlay());
        }

        [Test]
        public void TestShow()
        {
            AddStep("Show", overlay.Show);
        }

        [Test]
        public void TestHide()
        {
            AddStep("Hide", overlay.Hide);
        }
    }
}
