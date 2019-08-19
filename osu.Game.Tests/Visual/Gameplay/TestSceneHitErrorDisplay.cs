// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Screens.Play.HitErrorDisplay;
using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Framework.MathUtils;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneHitErrorDisplay : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(HitErrorDisplay),
        };

        private HitErrorDisplay display;

        public TestSceneHitErrorDisplay()
        {
            recreateDisplay(new OsuHitWindows(), 5);
            AddStep("New random judgement", () => newJudgement());
            AddStep("New fixed judgement (50ms)", () => newJudgement(50));
        }

        [Test]
        public void TestOsu()
        {
            AddStep("OD 1", () => recreateDisplay(new OsuHitWindows(), 1));
            AddStep("OD 2", () => recreateDisplay(new OsuHitWindows(), 2));
            AddStep("OD 3", () => recreateDisplay(new OsuHitWindows(), 3));
            AddStep("OD 4", () => recreateDisplay(new OsuHitWindows(), 4));
            AddStep("OD 5", () => recreateDisplay(new OsuHitWindows(), 5));
            AddStep("OD 6", () => recreateDisplay(new OsuHitWindows(), 6));
            AddStep("OD 7", () => recreateDisplay(new OsuHitWindows(), 7));
            AddStep("OD 8", () => recreateDisplay(new OsuHitWindows(), 8));
            AddStep("OD 9", () => recreateDisplay(new OsuHitWindows(), 9));
            AddStep("OD 10", () => recreateDisplay(new OsuHitWindows(), 10));
        }

        [Test]
        public void TestTaiko()
        {
            AddStep("OD 1", () => recreateDisplay(new TaikoHitWindows(), 1));
            AddStep("OD 2", () => recreateDisplay(new TaikoHitWindows(), 2));
            AddStep("OD 3", () => recreateDisplay(new TaikoHitWindows(), 3));
            AddStep("OD 4", () => recreateDisplay(new TaikoHitWindows(), 4));
            AddStep("OD 5", () => recreateDisplay(new TaikoHitWindows(), 5));
            AddStep("OD 6", () => recreateDisplay(new TaikoHitWindows(), 6));
            AddStep("OD 7", () => recreateDisplay(new TaikoHitWindows(), 7));
            AddStep("OD 8", () => recreateDisplay(new TaikoHitWindows(), 8));
            AddStep("OD 9", () => recreateDisplay(new TaikoHitWindows(), 9));
            AddStep("OD 10", () => recreateDisplay(new TaikoHitWindows(), 10));
        }

        [Test]
        public void TestMania()
        {
            AddStep("OD 1", () => recreateDisplay(new ManiaHitWindows(), 1));
            AddStep("OD 2", () => recreateDisplay(new ManiaHitWindows(), 2));
            AddStep("OD 3", () => recreateDisplay(new ManiaHitWindows(), 3));
            AddStep("OD 4", () => recreateDisplay(new ManiaHitWindows(), 4));
            AddStep("OD 5", () => recreateDisplay(new ManiaHitWindows(), 5));
            AddStep("OD 6", () => recreateDisplay(new ManiaHitWindows(), 6));
            AddStep("OD 7", () => recreateDisplay(new ManiaHitWindows(), 7));
            AddStep("OD 8", () => recreateDisplay(new ManiaHitWindows(), 8));
            AddStep("OD 9", () => recreateDisplay(new ManiaHitWindows(), 9));
            AddStep("OD 10", () => recreateDisplay(new ManiaHitWindows(), 10));
        }

        [Test]
        public void TestCatch()
        {
            AddStep("OD 1", () => recreateDisplay(new CatchHitWindows(), 1));
            AddStep("OD 2", () => recreateDisplay(new CatchHitWindows(), 2));
            AddStep("OD 3", () => recreateDisplay(new CatchHitWindows(), 3));
            AddStep("OD 4", () => recreateDisplay(new CatchHitWindows(), 4));
            AddStep("OD 5", () => recreateDisplay(new CatchHitWindows(), 5));
            AddStep("OD 6", () => recreateDisplay(new CatchHitWindows(), 6));
            AddStep("OD 7", () => recreateDisplay(new CatchHitWindows(), 7));
            AddStep("OD 8", () => recreateDisplay(new CatchHitWindows(), 8));
            AddStep("OD 9", () => recreateDisplay(new CatchHitWindows(), 9));
            AddStep("OD 10", () => recreateDisplay(new CatchHitWindows(), 10));
        }

        private void recreateDisplay(HitWindows hitWindows, float overallDifficulty)
        {
            hitWindows.SetDifficulty(overallDifficulty);

            Clear();

            Add(new FillFlowContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both,
                Children = new[]
                {
                    new SpriteText { Text = $@"Great: {hitWindows.Great}" },
                    new SpriteText { Text = $@"Good: {hitWindows.Good}" },
                    new SpriteText { Text = $@"Meh: {hitWindows.Meh}" },
                }
            });

            Add(display = new DefaultHitErrorDisplay(overallDifficulty, hitWindows)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        private void newJudgement(float offset = 0)
        {
            display?.OnNewJudgement(new JudgementResult(new Judgement())
            {
                TimeOffset = offset == 0 ? RNG.Next(-70, 70) : offset,
                Type = HitResult.Perfect,
            });
        }
    }
}
