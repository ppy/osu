// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Catch.Scoring;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Scoring;
using osu.Game.Screens.Play.HUD.HitErrorMeters;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneHitErrorMeter : OsuTestScene
    {
        private HitWindows hitWindows;

        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor();

        public TestSceneHitErrorMeter()
        {
            recreateDisplay(new OsuHitWindows(), 5);

            AddRepeatStep("New random judgement", () => newJudgement(), 40);

            AddRepeatStep("New max negative", () => newJudgement(-hitWindows.WindowFor(HitResult.Meh)), 20);
            AddRepeatStep("New max positive", () => newJudgement(hitWindows.WindowFor(HitResult.Meh)), 20);
            AddStep("New fixed judgement (50ms)", () => newJudgement(50));

            AddStep("Judgement barrage", () =>
            {
                int runCount = 0;

                ScheduledDelegate del = null;

                del = Scheduler.AddDelayed(() =>
                {
                    newJudgement(runCount++ / 10f);

                    if (runCount == 500)
                        // ReSharper disable once AccessToModifiedClosure
                        del?.Cancel();
                }, 10, true);
            });
        }

        [Test]
        public void TestOsu()
        {
            AddStep("OD 1", () => recreateDisplay(new OsuHitWindows(), 1));
            AddStep("OD 10", () => recreateDisplay(new OsuHitWindows(), 10));
        }

        [Test]
        public void TestTaiko()
        {
            AddStep("OD 1", () => recreateDisplay(new TaikoHitWindows(), 1));
            AddStep("OD 10", () => recreateDisplay(new TaikoHitWindows(), 10));
        }

        [Test]
        public void TestMania()
        {
            AddStep("OD 1", () => recreateDisplay(new ManiaHitWindows(), 1));
            AddStep("OD 10", () => recreateDisplay(new ManiaHitWindows(), 10));
        }

        [Test]
        public void TestCatch()
        {
            AddStep("OD 1", () => recreateDisplay(new CatchHitWindows(), 1));
            AddStep("OD 10", () => recreateDisplay(new CatchHitWindows(), 10));
        }

        private void recreateDisplay(HitWindows hitWindows, float overallDifficulty)
        {
            this.hitWindows = hitWindows;

            hitWindows?.SetDifficulty(overallDifficulty);

            Clear();

            Add(new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both,
                Children = new[]
                {
                    new OsuSpriteText { Text = $@"Great: {hitWindows?.WindowFor(HitResult.Great)}" },
                    new OsuSpriteText { Text = $@"Good: {hitWindows?.WindowFor(HitResult.Ok)}" },
                    new OsuSpriteText { Text = $@"Meh: {hitWindows?.WindowFor(HitResult.Meh)}" },
                }
            });

            Add(new BarHitErrorMeter(hitWindows, true)
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
            });

            Add(new BarHitErrorMeter(hitWindows, false)
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
            });

            Add(new BarHitErrorMeter(hitWindows, true)
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.CentreLeft,
                Rotation = 270,
            });

            Add(new ColourHitErrorMeter(hitWindows)
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Margin = new MarginPadding { Right = 50 }
            });

            Add(new ColourHitErrorMeter(hitWindows)
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Margin = new MarginPadding { Left = 50 }
            });

            Add(new ColourHitErrorMeter(hitWindows)
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.CentreLeft,
                Rotation = 270,
                Margin = new MarginPadding { Left = 50 }
            });
        }

        private void newJudgement(double offset = 0)
        {
            scoreProcessor.ApplyResult(new JudgementResult(new HitCircle { HitWindows = hitWindows }, new Judgement())
            {
                TimeOffset = offset == 0 ? RNG.Next(-150, 150) : offset,
                Type = HitResult.Perfect,
            });
        }
    }
}
