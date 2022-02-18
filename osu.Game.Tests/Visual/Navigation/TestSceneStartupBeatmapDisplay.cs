// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Navigation
{
    public class TestSceneStartupBeatmapDisplay : OsuGameTestScene
    {
        protected override TestOsuGame CreateTestGame() => new TestOsuGame(LocalStorage, API, new[] { "osu://b/75" });

        [Test]
        public void TestBeatmapLink()
        {
            AddUntilStep("Beatmap overlay displayed", () => Game.ChildrenOfType<BeatmapSetOverlay>().FirstOrDefault()?.State.Value == Visibility.Visible);
        }
    }
}
