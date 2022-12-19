// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneSkinnableComboCounter : SkinnableHUDComponentTestScene
    {
        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor(new OsuRuleset());

        protected override Drawable CreateDefaultImplementation() => new DefaultComboCounter();
        protected override Drawable CreateLegacyImplementation() => new LegacyComboCounter();

        [Test]
        public void TestComboCounterIncrementing()
        {
            AddRepeatStep("increase combo", () => scoreProcessor.Combo.Value++, 10);

            AddStep("reset combo", () => scoreProcessor.Combo.Value = 0);
        }

        [Test]
        public void TestLegacyComboCounterHiddenByRulesetImplementation()
        {
            AddToggleStep("toggle legacy hidden by ruleset", visible =>
            {
                foreach (var legacyCounter in this.ChildrenOfType<LegacyComboCounter>())
                    legacyCounter.HiddenByRulesetImplementation = visible;
            });

            AddRepeatStep("increase combo", () => scoreProcessor.Combo.Value++, 10);
        }
    }
}
