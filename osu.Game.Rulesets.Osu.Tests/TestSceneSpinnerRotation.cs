// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Testing;
using osu.Framework.Timing;
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
using osu.Game.Screens.Play;
using osu.Game.Storyboards;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneSpinnerRotation : TestSceneOsuPlayer
    {
        private const double spinner_start_time = 100;
        private const double spinner_duration = 6000;

        [Resolved]
        private AudioManager audioManager { get; set; } = null!;

        protected override bool Autoplay => true;

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new ScoreExposedPlayer();

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null)
            => new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audioManager);

        private DrawableSpinner drawableSpinner = null!;

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
            AddAssert("is disc rotation not almost 0", () => drawableSpinner.RotationTracker.Rotation, () => Is.Not.EqualTo(0).Within(100));
            AddAssert("is disc rotation absolute not almost 0", () => drawableSpinner.Result.TotalRotation, () => Is.Not.EqualTo(0).Within(100));

            addSeekStep(0);
            AddAssert("is disc rotation almost 0", () => drawableSpinner.RotationTracker.Rotation, () => Is.EqualTo(0).Within(trackerRotationTolerance));
            AddAssert("is disc rotation absolute almost 0", () => drawableSpinner.Result.TotalRotation, () => Is.EqualTo(0).Within(100));
        }

        [Test]
        public void TestSpinnerMiddleRewindingRotation()
        {
            double finalCumulativeTrackerRotation = 0;
            double finalTrackerRotation = 0, trackerRotationTolerance = 0;

            addSeekStep(spinner_start_time + 5000);
            AddStep("retrieve disc rotation", () =>
            {
                finalTrackerRotation = drawableSpinner.RotationTracker.Rotation;
                trackerRotationTolerance = Math.Abs(finalTrackerRotation * 0.05f);
            });
            AddStep("retrieve cumulative disc rotation", () => finalCumulativeTrackerRotation = drawableSpinner.Result.TotalRotation);

            addSeekStep(spinner_start_time + 2500);
            AddAssert("disc rotation rewound",
                // we want to make sure that the rotation at time 2500 is in the same direction as at time 5000, but about half-way in.
                // due to the exponential damping applied we're allowing a larger margin of error of about 10%
                // (5% relative to the final rotation value, but we're half-way through the spin).
                () => drawableSpinner.RotationTracker.Rotation, () => Is.EqualTo(finalTrackerRotation / 2).Within(trackerRotationTolerance));
            AddAssert("is cumulative rotation rewound",
                // cumulative rotation is not damped, so we're treating it as the "ground truth" and allowing a comparatively smaller margin of error.
                () => drawableSpinner.Result.TotalRotation, () => Is.EqualTo(finalCumulativeTrackerRotation / 2).Within(100));

            addSeekStep(spinner_start_time + 5000);
            AddAssert("is disc rotation almost same",
                () => drawableSpinner.RotationTracker.Rotation, () => Is.EqualTo(finalTrackerRotation).Within(trackerRotationTolerance));
            AddAssert("is cumulative rotation almost same",
                () => drawableSpinner.Result.TotalRotation, () => Is.EqualTo(finalCumulativeTrackerRotation).Within(100));
        }

        [Test]
        public void TestRotationDirection([Values(true, false)] bool clockwise)
        {
            if (clockwise)
                transformReplay(flip);

            addSeekStep(5000);

            AddAssert("disc spin direction correct", () => clockwise ? drawableSpinner.RotationTracker.Rotation > 0 : drawableSpinner.RotationTracker.Rotation < 0);
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
            addSeekStep(spinner_start_time + 1000);

            AddAssert("player score matching expected bonus score", () =>
            {
                // multipled by 2 to nullify the score multiplier. (autoplay mod selected)
                long totalScore = ((ScoreExposedPlayer)Player).ScoreProcessor.TotalScore.Value * 2;
                return totalScore == (int)(drawableSpinner.Result.TotalRotation / 360) * new SpinnerTick().CreateJudgement().MaxNumericResult;
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
            AddStep("retrieve spm", () => estimatedSpm = drawableSpinner.SpinsPerMinute.Value);

            addSeekStep(2000);
            AddAssert("spm still valid", () => drawableSpinner.SpinsPerMinute.Value, () => Is.EqualTo(estimatedSpm).Within(1.0));

            addSeekStep(1000);
            AddAssert("spm still valid", () => drawableSpinner.SpinsPerMinute.Value, () => Is.EqualTo(estimatedSpm).Within(1.0));
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
                expectedSpm = drawableSpinner.SpinsPerMinute.Value;
            });

            addSeekStep(0);

            AddStep("adjust track rate", () => ((MasterGameplayClockContainer)Player.GameplayClockContainer).UserPlaybackRate.Value = rate);

            addSeekStep(1000);
            AddAssert("progress almost same", () => drawableSpinner.Progress, () => Is.EqualTo(expectedProgress).Within(0.05));
            AddAssert("spm almost same", () => drawableSpinner.SpinsPerMinute.Value, () => Is.EqualTo(expectedSpm).Within(2.0));
        }

        private void addSeekStep(double time)
        {
            AddStep($"seek to {time}", () => Player.GameplayClockContainer.Seek(time));
            AddUntilStep("wait for seek to finish", () => Player.DrawableRuleset.FrameStableClock.CurrentTime, () => Is.EqualTo(time).Within(100));
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
                    StartTime = spinner_start_time,
                    Duration = spinner_duration
                },
            }
        };

        private partial class ScoreExposedPlayer : TestPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            public ScoreExposedPlayer()
                : base(false, false)
            {
            }
        }
    }
}
