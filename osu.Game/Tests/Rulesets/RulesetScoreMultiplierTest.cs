// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Tests.Rulesets
{
    [TestFixture]
    public abstract class RulesetScoreMultiplierTest
    {
        public Ruleset Ruleset { get; }

        protected RulesetScoreMultiplierTest(Ruleset ruleset)
        {
            Ruleset = ruleset;
        }

        [Test]
        public void TestDefaultMultiplierIsOne()
            => TestModCombination([], 1);

        protected void TestModCombination(IEnumerable<Mod> mods, double expectedMultiplier)
        {
            var calculator = Ruleset.CreateScoreMultiplierCalculator(new ScoreMultiplierContext(new BeatmapDifficulty()));
            Assert.That(calculator.CalculateFor(mods), Is.EqualTo(expectedMultiplier).Within(Precision.DOUBLE_EPSILON));
        }
    }
}
