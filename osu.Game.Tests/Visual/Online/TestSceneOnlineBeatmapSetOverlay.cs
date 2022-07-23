// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneOnlineBeatmapSetOverlay : OsuTestScene
    {
        private readonly BeatmapSetOverlay overlay;

        protected override bool UseOnlineAPI => true;

        public TestSceneOnlineBeatmapSetOverlay()
        {
            Add(overlay = new BeatmapSetOverlay());
        }

        [Test]
        public void TestOnline()
        {
            AddStep(@"show online", () => overlay.FetchAndShowBeatmapSet(55));
        }
    }
}
