// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osu.Game.Storyboards;
using osuTK;
using static osu.Game.Tests.Visual.OsuTestScene.ClockBackedTestWorkingBeatmap;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneSliderSnaking : TestSceneOsuPlayer
    {
        [Resolved]
        private AudioManager audioManager { get; set; }

        private TrackVirtualManual track;

        protected override bool Autoplay => autoplay;
        private bool autoplay;

        private readonly BindableBool snakingIn = new BindableBool();
        private readonly BindableBool snakingOut = new BindableBool();

        private const double duration_of_span = 3605;
        private const double fade_in_modifier = -1200;

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
        {
            var working = new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audioManager);
            track = (TrackVirtualManual)working.Track;
            return working;
        }

        [BackgroundDependencyLoader]
        private void load(RulesetConfigCache configCache)
        {
            var config = (OsuRulesetConfigManager)configCache.GetConfigFor(Ruleset.Value.CreateInstance());
            config.BindWith(OsuRulesetSetting.SnakingInSliders, snakingIn);
            config.BindWith(OsuRulesetSetting.SnakingOutSliders, snakingOut);
        }

        private DrawableSlider slider;

        [SetUpSteps]
        public override void SetUpSteps()
        {
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void TestSnakingEnabled(int sliderIndex)
        {
            AddStep("enable autoplay", () => autoplay = true);
            base.SetUpSteps();
            AddUntilStep("wait for track to start running", () => track.IsRunning);

            double startTime = hitObjects[sliderIndex].StartTime;
            retrieveDrawableSlider(sliderIndex);
            setSnaking(true);

            ensureSnakingIn(startTime + fade_in_modifier);

            for (int i = 0; i < sliderIndex; i++)
            {
                // non-final repeats should not snake out
                ensureNoSnakingOut(startTime, i);
            }

            // final repeat should snake out
            ensureSnakingOut(startTime, sliderIndex);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void TestSnakingDisabled(int sliderIndex)
        {
            AddStep("have autoplay", () => autoplay = true);
            base.SetUpSteps();
            AddUntilStep("wait for track to start running", () => track.IsRunning);

            double startTime = hitObjects[sliderIndex].StartTime;
            retrieveDrawableSlider(sliderIndex);
            setSnaking(false);

            ensureNoSnakingIn(startTime + fade_in_modifier);

            for (int i = 0; i <= sliderIndex; i++)
            {
                // no snaking out ever, including final repeat
                ensureNoSnakingOut(startTime, i);
            }
        }

        [Test]
        public void TestRepeatArrowDoesNotMoveWhenHit()
        {
            AddStep("enable autoplay", () => autoplay = true);
            setSnaking(true);
            base.SetUpSteps();

            // repeat might have a chance to update its position depending on where in the frame its hit,
            // so some leniency is allowed here instead of checking strict equality
            checkPositionChange(16600, sliderRepeat, positionAlmostSame);
        }

        [Test]
        public void TestRepeatArrowMovesWhenNotHit()
        {
            AddStep("disable autoplay", () => autoplay = false);
            setSnaking(true);
            base.SetUpSteps();

            checkPositionChange(16600, sliderRepeat, positionDecreased);
        }

        private void retrieveDrawableSlider(int index) =>
            AddStep($"retrieve {(index + 1).ToOrdinalWords()} slider", () =>
                slider = (DrawableSlider)Player.DrawableRuleset.Playfield.AllHitObjects.ElementAt(index));

        private void ensureSnakingIn(double startTime) => checkPositionChange(startTime, sliderEnd, positionIncreased);
        private void ensureNoSnakingIn(double startTime) => checkPositionChange(startTime, sliderEnd, positionRemainsSame);

        private void ensureSnakingOut(double startTime, int repeatIndex)
        {
            var repeatTime = timeAtRepeat(startTime, repeatIndex);

            if (repeatIndex % 2 == 0)
                checkPositionChange(repeatTime, sliderStart, positionIncreased);
            else
                checkPositionChange(repeatTime, sliderEnd, positionDecreased);
        }

        private void ensureNoSnakingOut(double startTime, int repeatIndex) =>
            checkPositionChange(timeAtRepeat(startTime, repeatIndex), positionAtRepeat(repeatIndex), positionRemainsSame);

        private double timeAtRepeat(double startTime, int repeatIndex) => startTime + 100 + duration_of_span * repeatIndex;
        private Func<Vector2> positionAtRepeat(int repeatIndex) => repeatIndex % 2 == 0 ? (Func<Vector2>)sliderStart : sliderEnd;

        private List<Vector2> sliderCurve => ((PlaySliderBody)slider.Body.Drawable).CurrentCurve;
        private Vector2 sliderStart() => sliderCurve.First();
        private Vector2 sliderEnd() => sliderCurve.Last();

        private Vector2 sliderRepeat()
        {
            var drawable = Player.DrawableRuleset.Playfield.AllHitObjects.ElementAt(1);
            var repeat = drawable.ChildrenOfType<Container<DrawableSliderRepeat>>().First().Children.First();
            return repeat.Position;
        }

        private bool positionRemainsSame(Vector2 previous, Vector2 current) => previous == current;
        private bool positionIncreased(Vector2 previous, Vector2 current) => current.X > previous.X && current.Y > previous.Y;
        private bool positionDecreased(Vector2 previous, Vector2 current) => current.X < previous.X && current.Y < previous.Y;
        private bool positionAlmostSame(Vector2 previous, Vector2 current) => Precision.AlmostEquals(previous, current, 1);

        private void checkPositionChange(double startTime, Func<Vector2> positionToCheck, Func<Vector2, Vector2, bool> positionAssertion)
        {
            Vector2 previousPosition = Vector2.Zero;

            string positionDescription = positionToCheck.Method.Name.Humanize(LetterCasing.LowerCase);
            string assertionDescription = positionAssertion.Method.Name.Humanize(LetterCasing.LowerCase);

            addSeekStep(startTime);
            AddStep($"save {positionDescription} position", () => previousPosition = positionToCheck.Invoke());
            addSeekStep(startTime + 100);
            AddAssert($"{positionDescription} {assertionDescription}", () =>
            {
                var currentPosition = positionToCheck.Invoke();
                return positionAssertion.Invoke(previousPosition, currentPosition);
            });
        }

        private void setSnaking(bool value)
        {
            AddStep($"{(value ? "enable" : "disable")} snaking", () =>
            {
                snakingIn.Value = value;
                snakingOut.Value = value;
            });
        }

        private void addSeekStep(double time)
        {
            AddStep($"seek to {time}", () => track.Seek(time));

            AddUntilStep("wait for seek to finish", () => Precision.AlmostEquals(time, Player.DrawableRuleset.FrameStableClock.CurrentTime, 100));
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap
        {
            HitObjects = hitObjects
        };

        private readonly List<HitObject> hitObjects = new List<HitObject>
        {
            new Slider
            {
                StartTime = 3000,
                Position = new Vector2(100, 100),
                Path = new SliderPath(PathType.PerfectCurve, new[]
                {
                    Vector2.Zero,
                    new Vector2(300, 200)
                }),
            },
            new Slider
            {
                StartTime = 13000,
                Position = new Vector2(100, 100),
                Path = new SliderPath(PathType.PerfectCurve, new[]
                {
                    Vector2.Zero,
                    new Vector2(300, 200)
                }),
                RepeatCount = 1,
            },
            new Slider
            {
                StartTime = 23000,
                Position = new Vector2(100, 100),
                Path = new SliderPath(PathType.PerfectCurve, new[]
                {
                    Vector2.Zero,
                    new Vector2(300, 200)
                }),
                RepeatCount = 2,
            },
            new HitCircle
            {
                StartTime = 199999,
            }
        };
    }
}
