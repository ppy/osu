// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinnableHUDComponents : SkinnableTestScene
    {
        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor();

        [Cached(typeof(HealthProcessor))]
        private HealthProcessor healthProcessor = new DrainingHealthProcessor(0);

        [BackgroundDependencyLoader]
        private void load()
        {
            SetContents(() => new SkinnableTargetContainer(SkinnableTarget.MainHUDComponents)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
            });
        }

        [Test]
        public void TestScoreCounter()
        {
            AddStep(@"reset total score", () => scoreProcessor.TotalScore.Value = 0);
            AddStep(@"increment total score", () => scoreProcessor.TotalScore.Value += 300);
            AddStep(@"set large score", () => scoreProcessor.TotalScore.Value = 1_000_000_000);
        }

        [Test]
        public void TestComboCounter()
        {
            AddStep(@"reset combo", () => scoreProcessor.Combo.Value = 0);
            AddRepeatStep(@"increase combo", () => scoreProcessor.Combo.Value++, 10);
        }

        [Test]
        public void TestAccuracyCounter()
        {
            AddStep(@"reset accuracy", () => scoreProcessor.Accuracy.Value = 1);
            AddStep(@"decrease accuracy", () => scoreProcessor.Accuracy.Value -= 0.023);
        }

        [Test]
        public void TestHealthDisplay()
        {
            AddStep(@"reset health", () => healthProcessor.Health.Value = 1);
            AddRepeatStep(@"decrease hp", () => healthProcessor.Health.Value -= 0.08f, 10);
            AddRepeatStep(@"decrease hp without flash", () => healthProcessor.Health.Value += 0.1f, 3);
            AddRepeatStep(@"increase hp with flash", () =>
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
