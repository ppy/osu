// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneComboCounter : SkinnableTestScene
    {
        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Create combo counters", () => SetContents(() => new SkinnableDrawable(new HUDSkinComponent(HUDSkinComponents.ComboCounter))));
        }

        [Test]
        public void TestComboCounterIncrementing()
        {
            AddRepeatStep("increase combo", () => scoreProcessor.Combo.Value++, 10);

            AddStep("reset combo", () => scoreProcessor.Combo.Value = 0);
        }
    }
}
