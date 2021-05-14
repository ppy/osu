// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinnableHealthDisplay : SkinnableTestScene
    {
        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [Cached(typeof(HealthProcessor))]
        private HealthProcessor healthProcessor = new DrainingHealthProcessor(0);

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Create health displays", () => SetContents(() => new SkinnableDrawable(new HUDSkinComponent(HUDSkinComponents.HealthDisplay))));
            AddStep(@"Reset all", delegate
            {
                healthProcessor.Health.Value = 1;
            });
        }

        [Test]
        public void TestHealthDisplayIncrementing()
        {
            AddRepeatStep(@"decrease hp", delegate
            {
                healthProcessor.Health.Value -= 0.08f;
            }, 10);

            AddRepeatStep(@"increase hp without flash", delegate
            {
                healthProcessor.Health.Value += 0.1f;
            }, 3);

            AddRepeatStep(@"increase hp with flash", delegate
            {
                healthProcessor.Health.Value += 0.1f;
                healthProcessor.ApplyResult(new JudgementResult(new HitCircle(), new OsuJudgement())
                {
                    Type = HitResult.Perfect
                });
            }, 3);
        }
    }
}
