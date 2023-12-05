// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneArgonHealthDisplay : OsuTestScene
    {
        [Cached(typeof(HealthProcessor))]
        private HealthProcessor healthProcessor = new DrainingHealthProcessor(0);

        private ArgonHealthDisplay healthDisplay = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddSliderStep("Height", 0, 64, 0, val =>
            {
                if (healthDisplay.IsNotNull())
                    healthDisplay.BarHeight.Value = val;
            });

            AddSliderStep("Width", 0, 1f, 0.98f, val =>
            {
                if (healthDisplay.IsNotNull())
                    healthDisplay.Width = val;
            });
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep(@"Reset all", delegate
            {
                healthProcessor.Health.Value = 1;
                healthProcessor.Failed += () => false; // health won't be updated if the processor gets into a "fail" state.

                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Gray,
                    },
                    healthDisplay = new ArgonHealthDisplay
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                };
            });
        }

        [Test]
        public void TestHealthDisplayIncrementing()
        {
            AddRepeatStep("apply miss judgement", applyMiss, 5);

            AddRepeatStep(@"decrease hp slightly", delegate
            {
                healthProcessor.Health.Value -= 0.01f;
            }, 10);

            AddRepeatStep(@"increase hp without flash", delegate
            {
                healthProcessor.Health.Value += 0.1f;
            }, 3);

            AddRepeatStep(@"increase hp with flash", delegate
            {
                healthProcessor.Health.Value += 0.1f;
                applyPerfectHit();
            }, 3);
        }

        private void applyPerfectHit()
        {
            healthProcessor.ApplyResult(new JudgementResult(new HitCircle(), new OsuJudgement())
            {
                Type = HitResult.Perfect
            });
        }

        [Test]
        public void TestLateMissAfterConsequentMisses()
        {
            AddUntilStep("wait for health", () => healthDisplay.Current.Value == 1);
            AddStep("apply sequence", () =>
            {
                for (int i = 0; i < 10; i++)
                    applyMiss();

                Scheduler.AddDelayed(applyMiss, 500 + 30);
            });
        }

        [Test]
        public void TestMissAlmostExactlyAfterLastMissAnimation()
        {
            AddUntilStep("wait for health", () => healthDisplay.Current.Value == 1);
            AddStep("apply sequence", () =>
            {
                const double interval = 500 + 15;

                for (int i = 0; i < 5; i++)
                {
                    if (i % 2 == 0)
                        Scheduler.AddDelayed(applyMiss, i * interval);
                    else
                    {
                        Scheduler.AddDelayed(applyMiss, i * interval);
                        Scheduler.AddDelayed(applyMiss, i * interval);
                    }
                }
            });
        }

        [Test]
        public void TestMissThenHitAtSameUpdateFrame()
        {
            AddUntilStep("wait for health", () => healthDisplay.Current.Value == 1);
            AddStep("set half health", () => healthProcessor.Health.Value = 0.5f);
            AddStep("apply miss and hit", () =>
            {
                applyMiss();
                applyMiss();
                applyPerfectHit();
                applyPerfectHit();
            });
            AddWaitStep("wait", 3);
            AddStep("apply miss and cancel with hit", () =>
            {
                applyMiss();
                applyPerfectHit();
                applyPerfectHit();
                applyPerfectHit();
                applyPerfectHit();
            });
        }

        private void applyMiss()
        {
            healthProcessor.ApplyResult(new JudgementResult(new HitObject(), new Judgement()) { Type = HitResult.Miss });
        }
    }
}
