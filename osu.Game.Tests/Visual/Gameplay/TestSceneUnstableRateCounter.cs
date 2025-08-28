// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning.Triangles;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneUnstableRateCounter : SkinnableHUDComponentTestScene
    {
        [Cached(typeof(ScoreProcessor))]
        private TestScoreProcessor scoreProcessor = new TestScoreProcessor();

        private readonly OsuHitWindows hitWindows;

        private double prev;

        protected override Drawable CreateDefaultImplementation() => new TrianglesUnstableRateCounter();
        protected override Drawable CreateArgonImplementation() => new ArgonUnstableRateCounter();
        protected override Drawable CreateLegacyImplementation() => Empty();

        public TestSceneUnstableRateCounter()
        {
            hitWindows = new OsuHitWindows();
            hitWindows.SetDifficulty(5);
        }

        public override void SetUpSteps()
        {
            AddStep("Reset Score Processor", () => scoreProcessor.Reset());
            base.SetUpSteps();
        }

        [Test]
        public void TestDisplay()
        {
            AddSliderStep("UR", 0, 2000, 0, v => this.ChildrenOfType<UnstableRateCounter>().ForEach(c => c.Current.Value = v));
            AddToggleStep("toggle validity", v => this.ChildrenOfType<UnstableRateCounter>().ForEach(c => c.IsValid.Value = v));
        }

        [Test]
        public void TestBasic()
        {
            // Needs multiples 2 by the nature of UR, and went for 4 to be safe.
            // Creates a 250 UR by placing a +25ms then a -25ms judgement, which then results in a 250 UR
            AddRepeatStep("Set UR to 250", () => applyJudgement(25, true), 4);

            AddUntilStep("UR = 250", () => this.ChildrenOfType<UnstableRateCounter>().All(c => c.Current.Value == 250));

            AddRepeatStep("Revert UR", () =>
            {
                scoreProcessor.RevertResult(
                    new JudgementResult(new HitCircle { HitWindows = hitWindows }, new Judgement())
                    {
                        GameplayRate = 1.0,
                        TimeOffset = 25,
                        Type = HitResult.Perfect,
                    });
            }, 4);

            AddUntilStep("UR is 0", () => this.ChildrenOfType<UnstableRateCounter>().All(c => c.Current.Value == 0));
            AddUntilStep("Counter is invalid", () => this.ChildrenOfType<UnstableRateCounter>().All(c => !c.IsValid.Value));

            //Sets a UR of 0 by creating 10 10ms offset judgements. Since average = offset, UR = 0
            AddRepeatStep("Set UR to 0", () => applyJudgement(10, false), 10);
            //Applies a UR of 100 by creating 10 -10ms offset judgements. At the 10th judgement, offset should be 100.
            AddRepeatStep("Bring UR to 100", () => applyJudgement(-10, false), 10);
        }

        [Test]
        public void TestCounterReceivesJudgementsBeforeCreation()
        {
            AddRepeatStep("Set UR to 250", () => applyJudgement(25, true), 4);

            AddUntilStep("UR = 250", () => this.ChildrenOfType<UnstableRateCounter>().All(c => c.Current.Value == 250));
        }

        [Test]
        public void TestStaticRateChange()
        {
            AddRepeatStep("Set UR to 250 at 1.5x", () => applyJudgement(25, true, 1.5), 4);

            AddUntilStep("UR = 250/1.5", () => this.ChildrenOfType<UnstableRateCounter>().All(c => c.Current.Value == (int)Math.Round(250.0 / 1.5)));
        }

        [Test]
        public void TestDynamicRateChange()
        {
            AddRepeatStep("Set UR to 100 at 1.0x", () => applyJudgement(10, true, 1.0), 4);
            AddRepeatStep("Bring UR to 100 at 1.5x", () => applyJudgement(15, true, 1.5), 4);

            AddUntilStep("UR = 100", () => this.ChildrenOfType<UnstableRateCounter>().All(c => c.Current.Value == 100));
        }

        private void applyJudgement(double offsetMs, bool alt, double gameplayRate = 1.0)
        {
            double placement = offsetMs;

            if (alt)
            {
                placement = prev > 0 ? -offsetMs : offsetMs;
                prev = placement;
            }

            scoreProcessor.ApplyResult(new JudgementResult(new HitCircle { HitWindows = hitWindows }, new Judgement())
            {
                TimeOffset = placement,
                GameplayRate = gameplayRate,
                Type = HitResult.Perfect,
            });
        }

        private partial class TestScoreProcessor : ScoreProcessor
        {
            public TestScoreProcessor()
                : base(new OsuRuleset())
            {
            }

            public void Reset() => base.Reset(false);
        }
    }
}
