// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

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
        {
            var calculator = Ruleset.CreateScoreMultiplierCalculator();
            Assert.That(calculator.CalculateFor([]), Is.EqualTo(1));
        }

        [Test]
        public void TestMultipliersMatchForIndividualMods()
        {
            var mods = Ruleset.CreateAllMods();
            var calculator = Ruleset.CreateScoreMultiplierCalculator();

            Assert.Multiple(() =>
            {
                foreach (var mod in mods)
                    Assert.That(calculator.CalculateFor(mod.Yield()), Is.EqualTo(mod.ScoreMultiplier), message: $"Score multiplier not matching for mod {mod.Name}");
            });
        }

        protected void TestModCombination(IEnumerable<Mod> mods)
        {
            var calculator = Ruleset.CreateScoreMultiplierCalculator();

            double expected = 1;
            foreach (var mod in mods)
                expected *= mod.ScoreMultiplier;

            Assert.That(calculator.CalculateFor(mods), Is.EqualTo(expected));
        }
    }
}
