// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Utils;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Game.Replays;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Scoring;
using osu.Game.Storyboards;
using static osu.Game.Tests.Visual.OsuTestScene.ClockBackedTestWorkingBeatmap;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneSpinnerRotation : TestSceneOsuPlayer
    {
        [Resolved]
        private AudioManager audioManager { get; set; }

        private TrackVirtualManual track;

        protected override bool Autoplay => true;

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
        {
            var working = new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audioManager);
            track = (TrackVirtualManual)working.Track;
            return working;
        }

        private DrawableSpinner drawableSpinner;
        private SpriteIcon spinnerSymbol => drawableSpinner.ChildrenOfType<SpriteIcon>().Single();

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddUntilStep("wait for track to start running", () => track.IsRunning);
            AddStep("retrieve spinner", () => drawableSpinner = (DrawableSpinner)Player.DrawableRuleset.Playfield.AllHitObjects.First());
        }

        [Test]
        public void TestSpinnerRewindingRotation()
        {
            addSeekStep(5000);
            AddAssert("is disc rotation not almost 0", () => !Precision.AlmostEquals(drawableSpinner.Disc.Rotation, 0, 100));
            AddAssert("is disc rotation absolute not almost 0", () => !Precision.AlmostEquals(drawableSpinner.Disc.CumulativeRotation, 0, 100));

            addSeekStep(0);
            AddAssert("is disc rotation almost 0", () => Precision.AlmostEquals(drawableSpinner.Disc.Rotation, 0, 100));
            AddAssert("is disc rotation absolute almost 0", () => Precision.AlmostEquals(drawableSpinner.Disc.CumulativeRotation, 0, 100));
        }

        [Test]
        public void TestSpinnerMiddleRewindingRotation()
        {
            double finalAbsoluteDiscRotation = 0, finalRelativeDiscRotation = 0, finalSpinnerSymbolRotation = 0;

            addSeekStep(5000);
            AddStep("retrieve disc relative rotation", () => finalRelativeDiscRotation = drawableSpinner.Disc.Rotation);
            AddStep("retrieve disc absolute rotation", () => finalAbsoluteDiscRotation = drawableSpinner.Disc.CumulativeRotation);
            AddStep("retrieve spinner symbol rotation", () => finalSpinnerSymbolRotation = spinnerSymbol.Rotation);

            addSeekStep(2500);
            AddUntilStep("disc rotation rewound",
                // we want to make sure that the rotation at time 2500 is in the same direction as at time 5000, but about half-way in.
                () => Precision.AlmostEquals(drawableSpinner.Disc.Rotation, finalRelativeDiscRotation / 2, 100));
            AddUntilStep("symbol rotation rewound",
                () => Precision.AlmostEquals(spinnerSymbol.Rotation, finalSpinnerSymbolRotation / 2, 100));

            addSeekStep(5000);
            AddAssert("is disc rotation almost same",
                () => Precision.AlmostEquals(drawableSpinner.Disc.Rotation, finalRelativeDiscRotation, 100));
            AddAssert("is symbol rotation almost same",
                () => Precision.AlmostEquals(spinnerSymbol.Rotation, finalSpinnerSymbolRotation, 100));
            AddAssert("is disc rotation absolute almost same",
                () => Precision.AlmostEquals(drawableSpinner.Disc.CumulativeRotation, finalAbsoluteDiscRotation, 100));
        }

        [Test]
        public void TestRotationDirection([Values(true, false)] bool clockwise)
        {
            if (clockwise)
            {
                AddStep("flip replay", () =>
                {
                    var drawableRuleset = this.ChildrenOfType<DrawableOsuRuleset>().Single();
                    var score = drawableRuleset.ReplayScore;
                    var scoreWithFlippedReplay = new Score
                    {
                        ScoreInfo = score.ScoreInfo,
                        Replay = flipReplay(score.Replay)
                    };
                    drawableRuleset.SetReplayScore(scoreWithFlippedReplay);
                });
            }

            addSeekStep(5000);

            AddAssert("disc spin direction correct", () => clockwise ? drawableSpinner.Disc.Rotation > 0 : drawableSpinner.Disc.Rotation < 0);
            AddAssert("spinner symbol direction correct", () => clockwise ? spinnerSymbol.Rotation > 0 : spinnerSymbol.Rotation < 0);
        }

        private Replay flipReplay(Replay scoreReplay) => new Replay
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
        public void TestSpinPerMinuteOnRewind()
        {
            double estimatedSpm = 0;

            addSeekStep(2500);
            AddStep("retrieve spm", () => estimatedSpm = drawableSpinner.SpmCounter.SpinsPerMinute);

            addSeekStep(5000);
            AddAssert("spm still valid", () => Precision.AlmostEquals(drawableSpinner.SpmCounter.SpinsPerMinute, estimatedSpm, 1.0));

            addSeekStep(2500);
            AddAssert("spm still valid", () => Precision.AlmostEquals(drawableSpinner.SpmCounter.SpinsPerMinute, estimatedSpm, 1.0));
        }

        private void addSeekStep(double time)
        {
            AddStep($"seek to {time}", () => track.Seek(time));

            AddUntilStep("wait for seek to finish", () => Precision.AlmostEquals(time, Player.DrawableRuleset.FrameStableClock.CurrentTime, 100));
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap
        {
            HitObjects = new List<HitObject>
            {
                new Spinner
                {
                    Position = new Vector2(256, 192),
                    EndTime = 6000,
                },
                // placeholder object to avoid hitting the results screen
                new HitCircle
                {
                    StartTime = 99999,
                }
            }
        };
    }
}
