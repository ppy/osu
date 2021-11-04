// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Graphics.Sprites;
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
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneUnstableRateCounter : OsuTestScene
    {
        [Cached(typeof(ScoreProcessor))]
        private TestScoreProcessor scoreProcessor = new TestScoreProcessor();

        [Cached(typeof(GameplayState))]
        private GameplayState gameplayState;


        [Cached(typeof(DrawableRuleset))]
        private TestDrawableRuleset drawableRuleset = new TestDrawableRuleset();

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
            AddStep("Create Display", () => recreateDisplay(new OsuHitWindows(), 5));


            AddRepeatStep("Set UR to 250ms", () => setUR(25), 20);

            AddStep("Reset UR", () =>
            {
                scoreProcessor.Reset();
                recreateDisplay(new OsuHitWindows(), 5);
            });

            AddRepeatStep("Set UR to 100ms", () => setUR(10), 20);

            AddStep("Reset UR", () =>
            {
                scoreProcessor.Reset();
                recreateDisplay(new OsuHitWindows(), 5);
            });

            AddRepeatStep("Set UR to 0 (+50ms)", () => newJudgement(50), 20);

            AddStep("Reset UR", () =>
            {
                scoreProcessor.Reset();
                recreateDisplay(new OsuHitWindows(), 5);
            });

            AddRepeatStep("Set UR to 0 (-50ms)", () => newJudgement(-50), 20);

            AddRepeatStep("New random judgement", () => newJudgement(), 40);
        }
        private void recreateDisplay(HitWindows hitWindows, float overallDifficulty)
        {
            hitWindows?.SetDifficulty(overallDifficulty);

            drawableRuleset.HitWindows = hitWindows;

            Clear();

            Add(new UnstableRateCounter
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(5),
            });

            Add(new BarHitErrorMeter
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.CentreLeft,
                Rotation = 270,
            });

            Add(new ColourHitErrorMeter
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.CentreLeft,
                Rotation = 270,
                Margin = new MarginPadding { Left = 50 }
            });
        }






        private void newJudgement(double offset = 0, HitResult result = HitResult.Perfect)
        {
            scoreProcessor.ApplyResult(new JudgementResult(new HitCircle { HitWindows = drawableRuleset.HitWindows }, new Judgement())
            {
                TimeOffset = offset == 0 ? RNG.Next(-150, 150) : offset,
                Type = result,
            });
        }

        private void setUR(double UR = 0, HitResult result = HitResult.Perfect)
        {

            double placement = prev > 0 ? -UR : UR;
            scoreProcessor.ApplyResult(new JudgementResult(new HitCircle { HitWindows = drawableRuleset.HitWindows }, new Judgement())
            {
                TimeOffset = placement,
                Type = result,
            });
            prev = placement;
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
            internal override bool FrameStablePlayback { get; set; }
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

        private class TestScoreProcessor : ScoreProcessor
        {
            public void Reset() => base.Reset(false);
        }
    }
}
