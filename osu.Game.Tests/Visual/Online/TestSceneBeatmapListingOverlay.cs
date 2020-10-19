// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays;
using NUnit.Framework;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneBeatmapListingOverlay : OsuTestScene
    {
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
