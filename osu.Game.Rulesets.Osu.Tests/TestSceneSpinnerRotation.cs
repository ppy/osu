// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Storyboards;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneSpinnerRotation : TestSceneOsuPlayer
    {
        [Resolved]
        private AudioManager audioManager { get; set; }

        protected override bool Autoplay => true;

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new ScoreExposedPlayer();

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
            => new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audioManager);

        private DrawableSpinner drawableSpinner;
        private SpriteIcon spinnerSymbol => drawableSpinner.ChildrenOfType<SpriteIcon>().Single();

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);
            AddStep("retrieve spinner", () => drawableSpinner = (DrawableSpinner)Player.DrawableRuleset.Playfield.AllHitObjects.First());
        }

        [Test]
        public void TestSpinnerRewindingRotation()
        {
            double trackerRotationTolerance = 0;

            addSeekStep(5000);
            AddStep("calculate rotation tolerance", () =>
            {
                trackerRotationTolerance = Math.Abs(drawableSpinner.RotationTracker.Rotation * 0.1f);
            });
            AddAssert("is disc rotation not almost 0", () => !Precision.AlmostEquals(drawableSpinner.RotationTracker.Rotation, 0, 100));
            AddAssert("is disc rotation absolute not almost 0", () => !Precision.AlmostEquals(drawableSpinner.Result.RateAdjustedRotation, 0, 100));

            addSeekStep(0);
            AddAssert("is disc rotation almost 0", () => Precision.AlmostEquals(drawableSpinner.RotationTracker.Rotation, 0, trackerRotationTolerance));
            AddAssert("is disc rotation absolute almost 0", () => Precision.AlmostEquals(drawableSpinner.Result.RateAdjustedRotation, 0, 100));
        }

        [Test]
        public void TestSpinnerMiddleRewindingRotation()
        {
            double finalCumulativeTrackerRotation = 0;
            double finalTrackerRotation = 0, trackerRotationTolerance = 0;
            double finalSpinnerSymbolRotation = 0, spinnerSymbolRotationTolerance = 0;

            addSeekStep(5000);
            AddStep("retrieve disc rotation", () =>
            {
                finalTrackerRotation = drawableSpinner.RotationTracker.Rotation;
                trackerRotationTolerance = Math.Abs(finalTrackerRotation * 0.05f);
            });
            AddStep("retrieve spinner symbol rotation", () =>
            {
                finalSpinnerSymbolRotation = spinnerSymbol.Rotation;
                spinnerSymbolRotationTolerance = Math.Abs(finalSpinnerSymbolRotation * 0.05f);
            });
            AddStep("retrieve cumulative disc rotation", () => finalCumulativeTrackerRotation = drawableSpinner.Result.RateAdjustedRotation);

            addSeekStep(2500);
            AddAssert("disc rotation rewound",
                // we want to make sure that the rotation at time 2500 is in the same direction as at time 5000, but about half-way in.
                // due to the exponential damping applied we're allowing a larger margin of error of about 10%
                // (5% relative to the final rotation value, but we're half-way through the spin).
                () => Precision.AlmostEquals(drawableSpinner.RotationTracker.Rotation, finalTrackerRotation / 2, trackerRotationTolerance));
            AddAssert("symbol rotation rewound",
                () => Precision.AlmostEquals(spinnerSymbol.Rotation, finalSpinnerSymbolRotation / 2, spinnerSymbolRotationTolerance));
            AddAssert("is cumulative rotation rewound",
                // cumulative rotation is not damped, so we're treating it as the "ground truth" and allowing a comparatively smaller margin of error.
                () => Precision.AlmostEquals(drawableSpinner.Result.RateAdjustedRotation, finalCumulativeTrackerRotation / 2, 100));

            addSeekStep(5000);
            AddAssert("is disc rotation almost same",
                () => Precision.AlmostEquals(drawableSpinner.RotationTracker.Rotation, finalTrackerRotation, trackerRotationTolerance));
            AddAssert("is symbol rotation almost same",
                () => Precision.AlmostEquals(spinnerSymbol.Rotation, finalSpinnerSymbolRotation, spinnerSymbolRotationTolerance));
            AddAssert("is cumulative rotation almost same",
                () => Precision.AlmostEquals(drawableSpinner.Result.RateAdjustedRotation, finalCumulativeTrackerRotation, 100));
        }

        [Test]
        public void TestRotationDirection([Values(true, false)] bool clockwise)
        {
            if (clockwise)
                transformReplay(flip);

            addSeekStep(5000);

            AddAssert("disc spin direction correct", () => clockwise ? drawableSpinner.RotationTracker.Rotation > 0 : drawableSpinner.RotationTracker.Rotation < 0);
            AddAssert("spinner symbol direction correct", () => clockwise ? spinnerSymbol.Rotation > 0 : spinnerSymbol.Rotation < 0);
        }

        private Replay flip(Replay scoreReplay) => new Replay
        {
            Frames = scoreReplay
                     .Frames
                     .Cast<OsuReplayFrame>()
                     .Select(replayFrame =>
                     {
                         var flippedPosition = new Vector2(OsuPlayfield.BASE_SIZE.X - replayFrame.Position.X, replayFrame.Position.Y);
                         return new OsuReplayFrame(replayFrame.Time, flippedPosition, replayFrame.Actions.ToArray());
                     })
                     .Cast<ReplayFrame>()
                     .ToList()
        };

        [Test]
        public void TestSpinnerNormalBonusRewinding()
        {
            addSeekStep(1000);

            AddAssert("player score matching expected bonus score", () =>
            {
                // multipled by 2 to nullify the score multiplier. (autoplay mod selected)
                var totalScore = ((ScoreExposedPlayer)Player).ScoreProcessor.TotalScore.Value * 2;
                return totalScore == (int)(drawableSpinner.Result.RateAdjustedRotation / 360) * new SpinnerTick().CreateJudgement().MaxNumericResult;
            });

            addSeekStep(0);

            AddAssert("player score is 0", () => ((ScoreExposedPlayer)Player).ScoreProcessor.TotalScore.Value == 0);
        }

        [Test]
        public void TestSpinnerCompleteBonusRewinding()
        {
            addSeekStep(2500);
            addSeekStep(0);

            AddAssert("player score is 0", () => ((ScoreExposedPlayer)Player).ScoreProcessor.TotalScore.Value == 0);
        }

        [Test]
        public void TestSpinPerMinuteOnRewind()
        {
            double estimatedSpm = 0;

            addSeekStep(1000);
            AddStep("retrieve spm", () => estimatedSpm = drawableSpinner.SpmCounter.SpinsPerMinute);

            addSeekStep(2000);
            AddAssert("spm still valid", () => Precision.AlmostEquals(drawableSpinner.SpmCounter.SpinsPerMinute, estimatedSpm, 1.0));

            addSeekStep(1000);
            AddAssert("spm still valid", () => Precision.AlmostEquals(drawableSpinner.SpmCounter.SpinsPerMinute, estimatedSpm, 1.0));
        }

        [TestCase(0.5)]
        [TestCase(2.0)]
        public void TestSpinUnaffectedByClockRate(double rate)
        {
            double expectedProgress = 0;
            double expectedSpm = 0;

            addSeekStep(1000);
            AddStep("retrieve spinner state", () =>
            {
                expectedProgress = drawableSpinner.Progress;
                expectedSpm = drawableSpinner.SpmCounter.SpinsPerMinute;
            });

            addSeekStep(0);

            AddStep("adjust track rate", () => Player.GameplayClockContainer.UserPlaybackRate.Value = rate);

            addSeekStep(1000);
            AddAssert("progress almost same", () => Precision.AlmostEquals(expectedProgress, drawableSpinner.Progress, 0.05));
            AddAssert("spm almost same", () => Precision.AlmostEquals(expectedSpm, drawableSpinner.SpmCounter.SpinsPerMinute, 2.0));
        }

        private Replay applyRateAdjustment(Replay scoreReplay, double rate) => new Replay
        {
            Frames = scoreReplay
                     .Frames
                     .Cast<OsuReplayFrame>()
                     .Select(replayFrame =>
                     {
                         var adjustedTime = replayFrame.Time * rate;
                         return new OsuReplayFrame(adjustedTime, replayFrame.Position, replayFrame.Actions.ToArray());
                     })
                     .Cast<ReplayFrame>()
                     .ToList()
        };

        private void addSeekStep(double time)
        {
            AddStep($"seek to {time}", () => MusicController.SeekTo(time));

            AddUntilStep("wait for seek to finish", () => Precision.AlmostEquals(time, Player.DrawableRuleset.FrameStableClock.CurrentTime, 100));
        }

        private void transformReplay(Func<Replay, Replay> replayTransformation) => AddStep("set replay", () =>
        {
            var drawableRuleset = this.ChildrenOfType<DrawableOsuRuleset>().Single();
            var score = drawableRuleset.ReplayScore;
            var transformedScore = new Score
            {
                ScoreInfo = score.ScoreInfo,
                Replay = replayTransformation.Invoke(score.Replay)
            };
            drawableRuleset.SetReplayScore(transformedScore);
        });

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap
        {
            HitObjects = new List<HitObject>
            {
                new Spinner
                {
                    Position = new Vector2(256, 192),
                    EndTime = 6000,
                },
            }
        };

        private class ScoreExposedPlayer : TestPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            public ScoreExposedPlayer()
                : base(false, false)
            {
            }
        }
    }
}
