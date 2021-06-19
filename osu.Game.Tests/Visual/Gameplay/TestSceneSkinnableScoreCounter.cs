// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinnableScoreCounter : SkinnableHUDComponentTestScene
    {
        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor();

        protected override Drawable CreateDefaultImplementation() => new DefaultScoreCounter();
        protected override Drawable CreateLegacyImplementation() => new LegacyScoreCounter();

        [Test]
        public void TestScoreCounterIncrementing()
        {
            AddStep(@"Reset all", () => scoreProcessor.TotalScore.Value = 0);

            AddStep(@"Hit! :D", () => scoreProcessor.TotalScore.Value += 300);
        }

        [Test]
        public void TestVeryLargeScore()
        {
            AddStep("set large score", () => scoreProcessor.TotalScore.Value = 1_000_000_000);
        }
    }
}
