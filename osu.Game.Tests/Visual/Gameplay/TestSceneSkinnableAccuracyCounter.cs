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
    public class TestSceneSkinnableAccuracyCounter : SkinnableTestScene
    {
        private IEnumerable<SkinnableAccuracyCounter> accuracyCounters => CreatedDrawables.OfType<SkinnableAccuracyCounter>();

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Create combo counters", () => SetContents(() =>
            {
                var accuracyCounter = new SkinnableAccuracyCounter();

                accuracyCounter.Current.Value = 1;

                return accuracyCounter;
            }));
        }

        [Test]
        public void TestChangingAccuracy()
        {
            AddStep(@"Reset all", delegate
            {
                foreach (var s in accuracyCounters)
                    s.Current.Value = 1;
            });

            AddStep(@"Hit! :D", delegate
            {
                foreach (var s in accuracyCounters)
                    s.Current.Value -= 0.023f;
            });
        }
    }
}
