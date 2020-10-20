// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinnableScoreCounter : SkinnableTestScene
    {
        private IEnumerable<SkinnableScoreCounter> scoreCounters => CreatedDrawables.OfType<SkinnableScoreCounter>();

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Create combo counters", () => SetContents(() =>
            {
                var comboCounter = new SkinnableScoreCounter();
                comboCounter.Current.Value = 1;
                return comboCounter;
            }));
        }

        [Test]
        public void TestScoreCounterIncrementing()
        {
            AddStep(@"Reset all", delegate
            {
                foreach (var s in scoreCounters)
                    s.Current.Value = 0;
            });

            AddStep(@"Hit! :D", delegate
            {
                foreach (var s in scoreCounters)
                    s.Current.Value += 300;
            });
        }

        [Test]
        public void TestVeryLargeScore()
        {
            AddStep("set large score", () => scoreCounters.ForEach(counter => counter.Current.Value = 1_000_000_000));
        }
    }
}
