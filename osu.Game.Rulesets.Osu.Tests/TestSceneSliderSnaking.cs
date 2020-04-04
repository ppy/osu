// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        private DrawableSliderRepeat repeat;
        private Vector2 savedVector;

        [SetUpSteps]
        public override void SetUpSteps() { }

        [Test]
        public void TestSnaking()
        {
            AddStep("have autoplay", () => autoplay = true);
            base.SetUpSteps();
            AddUntilStep("wait for track to start running", () => track.IsRunning);

            for (int i = 0; i < 3; i++)
            {
                testSlider(i, true);
                testSlider(i, false);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestArrowStays(bool isHit)
        {
            AddStep($"{(isHit ? "enable" : "disable")} autoplay", () => autoplay = isHit);
            setSnaking(true);
            base.SetUpSteps();

            addSeekStep(16500);
            AddStep("retrieve 2nd slider repeat", () =>
            {
                var drawable = Player.DrawableRuleset.Playfield.AllHitObjects.Skip(1).First();
                repeat = drawable.ChildrenOfType<Container<DrawableSliderRepeat>>().First().Children.First();
            });
            AddStep("Save repeat vector", () => savedVector = repeat.Position);
            addSeekStep(16700);

            AddAssert($"Repeat vector {(isHit ? "is same" : "decreased")}", () =>
            {
                if (isHit)
                    // Precision.AlmostEquals is used because repeat might have a chance to update its position depending on where in the frame its hit
                    return Precision.AlmostEquals(savedVector, repeat.Position, 1);

                return repeat.X < savedVector.X && repeat.Y < savedVector.Y;
            });
        }

        private void testSlider(int index, bool snaking)
        {
            double startTime = index * 10000 + 3000;
            int repeats = index;
            AddStep($"retrieve {(index + 1).ToOrdinalWords()} slider", () =>
            {
                slider = (DrawableSlider)Player.DrawableRuleset.Playfield.AllHitObjects.Skip(index).First();
            });
            setSnaking(snaking);
            testSnakingIn(startTime + fade_in_modifier, snaking);

            for (int i = 0; i < repeats + 1; i++)
            {
                testSnakingOut(startTime + 100 + duration_of_span * i, snaking && i == repeats, i % 2 == 1);
            }
        }

        private void testSnakingIn(double startTime, bool isSnakingExpected)
        {
            addSeekStep(startTime);
            AddStep("Save end vector", () => savedVector = getCurrentSliderVector(true));
            addSeekStep(startTime + 100);
            AddAssert($"End vector {(isSnakingExpected ? "increased" : "is same")}", () =>
            {
                var currentVector = getCurrentSliderVector(true);
                return isSnakingExpected ? currentVector.X > savedVector.X && currentVector.Y > savedVector.Y : currentVector == savedVector;
            });
        }

        private void testSnakingOut(double startTime, bool isSnakingExpected, bool testSliderEnd)
        {
            addSeekStep(startTime);
            AddStep($"Save {(testSliderEnd ? "end" : "start")} vector", () => savedVector = getCurrentSliderVector(testSliderEnd));
            addSeekStep(startTime + 100);
            AddAssert($"{(testSliderEnd ? "End" : "Start")} vector {(isSnakingExpected ? (testSliderEnd ? "decreased" : "increased") : "is same")}", () =>
            {
                var currentVector = getCurrentSliderVector(testSliderEnd);

                bool check(Vector2 a, Vector2 b)
                {
                    if (testSliderEnd)
                        return a.X < b.X && a.Y < b.Y;

                    return a.X > b.X && a.Y > b.Y;
                }

                return isSnakingExpected ? check(currentVector, savedVector) : currentVector == savedVector;
            });
        }

        private Vector2 getCurrentSliderVector(bool getEndOne)
        {
            var body = (PlaySliderBody)slider.Body.Drawable;
            return getEndOne ? body.CurrentCurve.Last() : body.CurrentCurve.First();
        }

        private void setSnaking(bool value)
        {
            AddStep($"{(value ? "Enable" : "Disable")} snaking", () =>
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
            HitObjects = new List<HitObject>
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
            }
        };
    }
}
