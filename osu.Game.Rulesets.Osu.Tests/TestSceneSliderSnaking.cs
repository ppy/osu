﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Storyboards;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneSliderSnaking : TestSceneOsuPlayer
    {
        [Resolved]
        private AudioManager audioManager { get; set; }

        protected override bool Autoplay => autoplay;
        private bool autoplay;

        private readonly BindableBool snakingIn = new BindableBool();
        private readonly BindableBool snakingOut = new BindableBool();

        private IBeatmap beatmap;

        private const double duration_of_span = 3605;
        private const double fade_in_modifier = -1200;

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
            => new ClockBackedTestWorkingBeatmap(this.beatmap = beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audioManager);

        [BackgroundDependencyLoader]
        private void load(RulesetConfigCache configCache)
        {
            var config = (OsuRulesetConfigManager)configCache.GetConfigFor(Ruleset.Value.CreateInstance());
            config.BindWith(OsuRulesetSetting.SnakingInSliders, snakingIn);
            config.BindWith(OsuRulesetSetting.SnakingOutSliders, snakingOut);
        }

        private Slider slider;
        private DrawableSlider drawableSlider;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            slider = null;
            drawableSlider = null;
        });

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
            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);

            retrieveSlider(sliderIndex);
            setSnaking(true);

            addEnsureSnakingInSteps(() => slider.StartTime + fade_in_modifier);

            for (int i = 0; i < sliderIndex; i++)
            {
                // non-final repeats should not snake out
                addEnsureNoSnakingOutStep(() => slider.StartTime, i);
            }

            // final repeat should snake out
            addEnsureSnakingOutSteps(() => slider.StartTime, sliderIndex);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void TestSnakingDisabled(int sliderIndex)
        {
            AddStep("have autoplay", () => autoplay = true);
            base.SetUpSteps();
            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);

            retrieveSlider(sliderIndex);
            setSnaking(false);

            addEnsureNoSnakingInSteps(() => slider.StartTime + fade_in_modifier);

            for (int i = 0; i <= sliderIndex; i++)
            {
                // no snaking out ever, including final repeat
                addEnsureNoSnakingOutStep(() => slider.StartTime, i);
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
            addCheckPositionChangeSteps(() => 16600, getSliderRepeat, positionAlmostSame);
        }

        [Test]
        public void TestRepeatArrowMovesWhenNotHit()
        {
            AddStep("disable autoplay", () => autoplay = false);
            setSnaking(true);
            base.SetUpSteps();

            addCheckPositionChangeSteps(() => 16600, getSliderRepeat, positionDecreased);
        }

        private void retrieveSlider(int index)
        {
            AddStep("retrieve slider at index", () => slider = (Slider)beatmap.HitObjects[index]);
            addSeekStep(() => slider);
            AddUntilStep("retrieve drawable slider", () =>
                (drawableSlider = (DrawableSlider)Player.DrawableRuleset.Playfield.AllHitObjects.SingleOrDefault(d => d.HitObject == slider)) != null);
        }

        private void addEnsureSnakingInSteps(Func<double> startTime) => addCheckPositionChangeSteps(startTime, getSliderEnd, positionIncreased);
        private void addEnsureNoSnakingInSteps(Func<double> startTime) => addCheckPositionChangeSteps(startTime, getSliderEnd, positionRemainsSame);

        private void addEnsureSnakingOutSteps(Func<double> startTime, int repeatIndex)
        {
            if (repeatIndex % 2 == 0)
                addCheckPositionChangeSteps(timeAtRepeat(startTime, repeatIndex), getSliderStart, positionIncreased);
            else
                addCheckPositionChangeSteps(timeAtRepeat(startTime, repeatIndex), getSliderEnd, positionDecreased);
        }

        private void addEnsureNoSnakingOutStep(Func<double> startTime, int repeatIndex)
            => addCheckPositionChangeSteps(timeAtRepeat(startTime, repeatIndex), positionAtRepeat(repeatIndex), positionRemainsSame);

        private Func<double> timeAtRepeat(Func<double> startTime, int repeatIndex) => () => startTime() + 100 + duration_of_span * repeatIndex;
        private Func<Vector2> positionAtRepeat(int repeatIndex) => repeatIndex % 2 == 0 ? (Func<Vector2>)getSliderStart : getSliderEnd;

        private List<Vector2> getSliderCurve() => ((PlaySliderBody)drawableSlider.Body.Drawable).CurrentCurve;
        private Vector2 getSliderStart() => getSliderCurve().First();
        private Vector2 getSliderEnd() => getSliderCurve().Last();

        private Vector2 getSliderRepeat()
        {
            var drawable = Player.DrawableRuleset.Playfield.AllHitObjects.SingleOrDefault(d => d.HitObject == beatmap.HitObjects[1]);
            var repeat = drawable.ChildrenOfType<Container<DrawableSliderRepeat>>().First().Children.First();
            return repeat.Position;
        }

        private bool positionRemainsSame(Vector2 previous, Vector2 current) => previous == current;
        private bool positionIncreased(Vector2 previous, Vector2 current) => current.X > previous.X && current.Y > previous.Y;
        private bool positionDecreased(Vector2 previous, Vector2 current) => current.X < previous.X && current.Y < previous.Y;
        private bool positionAlmostSame(Vector2 previous, Vector2 current) => Precision.AlmostEquals(previous, current, 1);

        private void addCheckPositionChangeSteps(Func<double> startTime, Func<Vector2> positionToCheck, Func<Vector2, Vector2, bool> positionAssertion)
        {
            Vector2 previousPosition = Vector2.Zero;

            string positionDescription = positionToCheck.Method.Name.Humanize(LetterCasing.LowerCase);
            string assertionDescription = positionAssertion.Method.Name.Humanize(LetterCasing.LowerCase);

            addSeekStep(startTime);
            AddStep($"save {positionDescription} position", () => previousPosition = positionToCheck.Invoke());
            addSeekStep(() => startTime() + 100);
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

        private void addSeekStep(Func<Slider> slider)
        {
            AddStep("seek to slider", () => Player.GameplayClockContainer.Seek(slider().StartTime));
            AddUntilStep("wait for seek to finish", () => Precision.AlmostEquals(slider().StartTime, Player.DrawableRuleset.FrameStableClock.CurrentTime, 100));
        }

        private void addSeekStep(Func<double> time)
        {
            AddStep("seek to time", () => Player.GameplayClockContainer.Seek(time()));
            AddUntilStep("wait for seek to finish", () => Precision.AlmostEquals(time(), Player.DrawableRuleset.FrameStableClock.CurrentTime, 100));
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap { HitObjects = createHitObjects() };

        private static List<HitObject> createHitObjects() => new List<HitObject>
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
