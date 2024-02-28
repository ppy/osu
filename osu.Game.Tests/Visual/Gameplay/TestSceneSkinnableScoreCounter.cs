// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osu.Game.Tests.Gameplay;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneSkinnableScoreCounter : SkinnableHUDComponentTestScene
    {
        [Cached(typeof(ScoreProcessor))]
        private ScoreProcessor scoreProcessor = TestGameplayState.Create(new OsuRuleset()).ScoreProcessor;

        protected override Drawable CreateArgonImplementation() => new ArgonScoreCounter();
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
