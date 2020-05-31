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
            AddAssert("is rotation absolute not almost 0", () => !Precision.AlmostEquals(drawableSpinner.Disc.RotationAbsolute, 0, 100));

            addSeekStep(0);
            AddAssert("is rotation absolute almost 0", () => Precision.AlmostEquals(drawableSpinner.Disc.RotationAbsolute, 0, 100));
        }

        [Test]
        public void TestSpinnerMiddleRewindingRotation()
        {
            double estimatedRotation = 0;

            addSeekStep(5000);
            AddStep("retrieve rotation", () => estimatedRotation = drawableSpinner.Disc.RotationAbsolute);

            addSeekStep(2500);
            addSeekStep(5000);
            AddAssert("is rotation absolute almost same", () => Precision.AlmostEquals(drawableSpinner.Disc.RotationAbsolute, estimatedRotation, 100));
        }

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
