// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD.HitErrorMeters;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneHitErrorMeter : OsuTestScene
    {
        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor();

        [Cached(typeof(DrawableRuleset))]
        private TestDrawableRuleset drawableRuleset = new TestDrawableRuleset();

        public TestSceneHitErrorMeter()
        {
            recreateDisplay(new OsuHitWindows(), 5);

            AddRepeatStep("New random judgement", () => newJudgement(), 40);

            AddRepeatStep("New max negative", () => newJudgement(-drawableRuleset.HitWindows.WindowFor(HitResult.Meh)), 20);
            AddRepeatStep("New max positive", () => newJudgement(drawableRuleset.HitWindows.WindowFor(HitResult.Meh)), 20);
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
            hitWindows?.SetDifficulty(overallDifficulty);

            drawableRuleset.HitWindows = hitWindows;

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

            Add(new BarHitErrorMeter
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
            });

            Add(new BarHitErrorMeter
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
            });

            Add(new BarHitErrorMeter
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.CentreLeft,
                Rotation = 270,
            });

            Add(new ColourHitErrorMeter
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Margin = new MarginPadding { Right = 50 }
            });

            Add(new ColourHitErrorMeter
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Margin = new MarginPadding { Left = 50 }
            });

            Add(new ColourHitErrorMeter
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.CentreLeft,
                Rotation = 270,
                Margin = new MarginPadding { Left = 50 }
            });
        }

        private void newJudgement(double offset = 0)
        {
            scoreProcessor.ApplyResult(new JudgementResult(new HitCircle { HitWindows = drawableRuleset.HitWindows }, new Judgement())
            {
                TimeOffset = offset == 0 ? RNG.Next(-150, 150) : offset,
                Type = HitResult.Perfect,
            });
        }

        [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
        private class TestDrawableRuleset : DrawableRuleset
        {
            public HitWindows HitWindows;

            public override IEnumerable<HitObject> Objects => new[] { new HitCircle { HitWindows = HitWindows } };

            public override event Action<JudgementResult> NewResult;
            public override event Action<JudgementResult> RevertResult;

            public override Playfield Playfield { get; }
            public override Container Overlays { get; }
            public override Container FrameStableComponents { get; }
            public override IFrameStableClock FrameStableClock { get; }
            public override IReadOnlyList<Mod> Mods { get; }

            public override double GameplayStartTime { get; }
            public override GameplayCursorContainer Cursor { get; }

            public TestDrawableRuleset()
                : base(new OsuRuleset())
            {
                // won't compile without this.
                NewResult?.Invoke(null);
                RevertResult?.Invoke(null);
            }

            public override void SetReplayScore(Score replayScore) => throw new NotImplementedException();

            public override void SetRecordTarget(Score score) => throw new NotImplementedException();

            public override void RequestResume(Action continueResume) => throw new NotImplementedException();

            public override void CancelResume() => throw new NotImplementedException();
        }
    }
}
