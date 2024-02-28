// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays;
using NUnit.Framework;

namespace osu.Game.Tests.Visual.Online
{
    [Description("uses online API")]
    public partial class TestSceneOnlineBeatmapListingOverlay : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        private readonly BeatmapListingOverlay overlay;

        public TestSceneOnlineBeatmapListingOverlay()
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
