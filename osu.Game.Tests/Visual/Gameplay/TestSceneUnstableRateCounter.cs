// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneUnstableRateCounter : OsuTestScene
    {
        [Cached(typeof(ScoreProcessor))]
        private TestScoreProcessor scoreProcessor = new TestScoreProcessor();

        private readonly OsuHitWindows hitWindows;

        private UnstableRateCounter counter;

        private double prev;

        public TestSceneUnstableRateCounter()
        {
            hitWindows = new OsuHitWindows();
            hitWindows.SetDifficulty(5);
        }

        [SetUpSteps]
        public void SetUp()
        {
            AddStep("Reset Score Processor", () => scoreProcessor.Reset());
        }

        [Test]
        public void TestBasic()
        {
            AddStep("Create Display", recreateDisplay);

            // Needs multiples 2 by the nature of UR, and went for 4 to be safe.
            // Creates a 250 UR by placing a +25ms then a -25ms judgement, which then results in a 250 UR
            AddRepeatStep("Set UR to 250", () => applyJudgement(25, true), 4);

            AddUntilStep("UR = 250", () => counter.Current.Value == 250.0);

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

            AddUntilStep("UR is 0", () => counter.Current.Value == 0.0);
            AddUntilStep("Counter is invalid", () => counter.Child.Alpha == 0.3f);

            //Sets a UR of 0 by creating 10 10ms offset judgements. Since average = offset, UR = 0
            AddRepeatStep("Set UR to 0", () => applyJudgement(10, false), 10);
            //Applies a UR of 100 by creating 10 -10ms offset judgements. At the 10th judgement, offset should be 100.
            AddRepeatStep("Bring UR to 100", () => applyJudgement(-10, false), 10);
        }

        [Test]
        public void TestCounterReceivesJudgementsBeforeCreation()
        {
            AddRepeatStep("Set UR to 250", () => applyJudgement(25, true), 4);

            AddStep("Create Display", recreateDisplay);

            AddUntilStep("UR = 250", () => counter.Current.Value == 250.0);
        }

        [Test]
        public void TestStaticRateChange()
        {
            AddStep("Create Display", recreateDisplay);

            AddRepeatStep("Set UR to 250 at 1.5x", () => applyJudgement(25, true, 1.5), 4);

            AddUntilStep("UR = 250/1.5", () => counter.Current.Value == Math.Round(250.0 / 1.5));
        }

        [Test]
        public void TestDynamicRateChange()
        {
            AddStep("Create Display", recreateDisplay);

            AddRepeatStep("Set UR to 100 at 1.0x", () => applyJudgement(10, true, 1.0), 4);
            AddRepeatStep("Bring UR to 100 at 1.5x", () => applyJudgement(15, true, 1.5), 4);

            AddUntilStep("UR = 100", () => counter.Current.Value == 100.0);
        }

        private void recreateDisplay()
        {
            Clear();

            Add(counter = new UnstableRateCounter
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(5),
            });
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
