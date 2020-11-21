// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneComboCounter : SkinnableTestScene
    {
        private IEnumerable<SkinnableComboCounter> comboCounters => CreatedDrawables.OfType<SkinnableComboCounter>();

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Create combo counters", () => SetContents(() =>
            {
                var comboCounter = new SkinnableComboCounter();
                comboCounter.Current.Value = 1;
                return comboCounter;
            }));
        }

        [Test]
        public void TestComboCounterIncrementing()
        {
            AddRepeatStep("increase combo", () =>
            {
                foreach (var counter in comboCounters)
                    counter.Current.Value++;
            }, 10);

            AddStep("reset combo", () =>
            {
                foreach (var counter in comboCounters)
                    counter.Current.Value = 0;
            });
        }
    }
}
