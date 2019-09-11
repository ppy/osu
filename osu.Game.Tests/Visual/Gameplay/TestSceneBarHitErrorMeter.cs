// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Objects;
using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Judgements;
using osu.Framework.MathUtils;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Catch.Scoring;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Scoring;
using osu.Game.Screens.Play.HUD.HitErrorMeters;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneBarHitErrorMeter : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(HitErrorMeter),
        };

        private HitErrorMeter meter;
        private HitErrorMeter meter2;
        private HitWindows hitWindows;

        public TestSceneBarHitErrorMeter()
        {
            recreateDisplay(new OsuHitWindows(), 5);

            AddRepeatStep("New random judgement", () => newJudgement(), 40);

            AddRepeatStep("New max negative", () => newJudgement(-hitWindows.WindowFor(HitResult.Meh)), 20);
            AddRepeatStep("New max positive", () => newJudgement(hitWindows.WindowFor(HitResult.Meh)), 20);
            AddStep("New fixed judgement (50ms)", () => newJudgement(50));
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
                    new SpriteText { Text = $@"Great: {hitWindows?.WindowFor(HitResult.Great)}" },
                    new SpriteText { Text = $@"Good: {hitWindows?.WindowFor(HitResult.Good)}" },
                    new SpriteText { Text = $@"Meh: {hitWindows?.WindowFor(HitResult.Meh)}" },
                }
            });

            Add(meter = new BarHitErrorMeter(hitWindows, true)
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
            });

            Add(meter2 = new BarHitErrorMeter(hitWindows, false)
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
            });
        }

        private void newJudgement(double offset = 0)
        {
            var judgement = new JudgementResult(new HitObject(), new Judgement())
            {
                TimeOffset = offset == 0 ? RNG.Next(-150, 150) : offset,
                Type = HitResult.Perfect,
            };

            meter.OnNewJudgement(judgement);
            meter2.OnNewJudgement(judgement);
        }
    }
}
