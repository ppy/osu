// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneUnstableRateCounter : OsuTestScene
    {
        [Cached(typeof(ScoreProcessor))]
        private TestScoreProcessor scoreProcessor = new TestScoreProcessor();

        [Cached(typeof(GameplayState))]
        private GameplayState gameplayState;

        private OsuHitWindows hitWindows = new OsuHitWindows();

        private double prev;

        public TestSceneUnstableRateCounter()
        {
            Score score = new Score
            {
                ScoreInfo = new ScoreInfo(),
            };
            gameplayState = new GameplayState(null, null, null, score);
            scoreProcessor.NewJudgement += result => scoreProcessor.PopulateScore(score.ScoreInfo);
        }

        [SetUpSteps]
        public void SetUp()
        {
            AddStep("Reset Score Processor", () => scoreProcessor.Reset());
        }

        [Test]
        public void TestBasic()
        {
            AddStep("Create Display", () => recreateDisplay());

            AddRepeatStep("Set UR to 250", () => setUR(25), 20);

            AddStep("Reset UR", () =>
            {
                scoreProcessor.Reset();
                recreateDisplay();
            });

            AddRepeatStep("Set UR to 100", () => setUR(10), 20);

            AddStep("Reset UR", () =>
            {
                scoreProcessor.Reset();
                recreateDisplay();
            });

            AddRepeatStep("Set UR to 0 (+50ms offset)", () => newJudgement(50), 10);

            AddStep("Reset UR", () =>
            {
                scoreProcessor.Reset();
                recreateDisplay();
            });

            AddRepeatStep("Set UR to 0 (-50 offset)", () => newJudgement(-50), 10);

            AddRepeatStep("Random Judgements", () => newJudgement(), 20);
        }
        private void recreateDisplay()
        {
            Clear();

            Add(new UnstableRateCounter
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(5),
            });
        }

        private void newJudgement(double offset = 0, HitResult result = HitResult.Perfect)
        {
            scoreProcessor.ApplyResult(new JudgementResult(new HitCircle { HitWindows = hitWindows }, new Judgement())
            {
                TimeOffset = offset == 0 ? RNG.Next(-150, 150) : offset,
                Type = result,
            });
        }

        private void setUR(double UR = 0, HitResult result = HitResult.Perfect)
        {
            double placement = prev > 0 ? -UR : UR;
            scoreProcessor.ApplyResult(new JudgementResult(new HitCircle { HitWindows = hitWindows }, new Judgement())
            {
                TimeOffset = placement,
                Type = result,
            });
            prev = placement;
        }

        private class TestScoreProcessor : ScoreProcessor
        {
            public void Reset() => base.Reset(false);
        }
    }
}
