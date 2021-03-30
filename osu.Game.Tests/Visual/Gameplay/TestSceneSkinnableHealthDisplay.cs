// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinnableHealthDisplay : SkinnableTestScene
    {
        private IEnumerable<SkinnableHealthDisplay> healthDisplays => CreatedDrawables.OfType<SkinnableHealthDisplay>();

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Create health displays", () =>
            {
                SetContents(() => new SkinnableHealthDisplay());
            });
            AddStep(@"Reset all", delegate
            {
                foreach (var s in healthDisplays)
                    s.Current.Value = 1;
            });
        }

        [Test]
        public void TestHealthDisplayIncrementing()
        {
            AddRepeatStep(@"decrease hp", delegate
            {
                foreach (var healthDisplay in healthDisplays)
                    healthDisplay.Current.Value -= 0.08f;
            }, 10);

            AddRepeatStep(@"increase hp without flash", delegate
            {
                foreach (var healthDisplay in healthDisplays)
                    healthDisplay.Current.Value += 0.1f;
            }, 3);

            AddRepeatStep(@"increase hp with flash", delegate
            {
                foreach (var healthDisplay in healthDisplays)
                {
                    healthDisplay.Current.Value += 0.1f;
                    healthDisplay.Flash(new JudgementResult(null, new OsuJudgement()));
                }
            }, 3);
        }
    }
}
