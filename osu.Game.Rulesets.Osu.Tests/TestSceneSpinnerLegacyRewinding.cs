// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Skinning.Legacy;
using osu.Game.Storyboards;
using osu.Game.Tests;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneSpinnerLegacyRewinding : LegacySkinPlayerTestScene
    {
        private const double spinner_start_time = 100;
        private const double spinner_duration = 6000;
        private const double spinner_end_time = spinner_start_time + spinner_duration;

        [Resolved]
        private AudioManager audioManager { get; set; } = null!;

        protected override bool Autoplay => true;

        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null)
            => new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audioManager);

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap
        {
            HitObjects = new List<HitObject>
            {
                new Spinner
                {
                    Position = new Vector2(256, 192),
                    StartTime = spinner_start_time,
                    Duration = spinner_duration,
                },
            }
        };

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);
        }

        /// <summary>
        /// The spinner is pooled, so it must be re-retrieved after each seek rather than held onto
        /// (it is detached from its judgement result once it expires).
        /// </summary>
        private DrawableSpinner? spinner => Player.DrawableRuleset.Playfield.AllHitObjects.OfType<DrawableSpinner>().SingleOrDefault();

        /// <summary>
        /// Regression test for https://github.com/ppy/osu/issues/36507.
        /// </summary>
        [Test]
        public void TestClearSpriteShownWhenRewindingIntoCompletedSpinner()
        {
            addPlayThroughSpinnerSteps();

            // rewind to a point after completion but before the spinner's end, where "clear" should still be displayed.
            addSeekStep(spinner_end_time - 100);
            AddUntilStep("wait for spinner alive", () => spinner != null);
            AddAssert("spinner was completed", () => spinner!.Result.TimeCompleted != null);

            AddAssert("clear sprite visible", () => legacySpinner.ClearSprite.Alpha, () => Is.GreaterThan(0));
        }

        /// <summary>
        /// Regression test for https://github.com/ppy/osu/issues/36508.
        /// </summary>
        [Test]
        public void TestSpinSpriteHiddenWhenRewindingIntoCompletedSpinner()
        {
            addPlayThroughSpinnerSteps();

            // "SPIN!" should not come back once the spinner has already been spun into completion.
            addSeekStep(spinner_end_time - 100);
            AddUntilStep("wait for spinner alive", () => spinner != null);
            AddAssert("spinner was completed", () => spinner!.Result.TimeCompleted != null);

            AddAssert("spin sprite hidden", () => legacySpinner.SpinSprite.Alpha, () => Is.EqualTo(0));
        }


        private LegacySpinner legacySpinner => Player.ChildrenOfType<LegacySpinner>().Single();

        /// <summary>
        /// Seeks through the whole spinner so that it is spun into completion and judged, then leaves playback
        /// just past its end.
        /// </summary>
        private void addPlayThroughSpinnerSteps()
        {
            addSeekStep(spinner_end_time + 1000);
        }

        private void addSeekStep(double time)
        {
            AddStep($"seek to {time}", () => Player.GameplayClockContainer.Seek(time));
            AddUntilStep("wait for seek to finish", () => Player.DrawableRuleset.FrameStableClock.CurrentTime, () => Is.EqualTo(time).Within(100));
        }
    }
}
