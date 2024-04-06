// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public partial class TestSceneOsuReverseSelection : TestSceneOsuEditor
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        [Test]
        public void TestReverseSelectionTwoCircles()
        {
            OsuHitObject[] objects = null!;
            bool[] newCombos = null!;

            AddStep("Add circles", () =>
            {
                var circle1 = new HitCircle
                {
                    StartTime = 0,
                    Position = new Vector2(208, 240)
                };
                var circle2 = new HitCircle
                {
                    StartTime = 200,
                    Position = new Vector2(256, 144)
                };

                EditorBeatmap.AddRange([circle1, circle2]);
            });

            AddStep("store objects & new combo data", () =>
            {
                objects = getObjects().ToArray();
                newCombos = getObjectNewCombos().ToArray();
            });

            AddStep("Select circles", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));

            AddStep("Reverse selection", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.G);
                InputManager.ReleaseKey(Key.LControl);
            });

            AddAssert("objects reversed", getObjects, () => Is.EqualTo(objects.Reverse()));
            AddAssert("new combo positions preserved", getObjectNewCombos, () => Is.EqualTo(newCombos));
        }

        [Test]
        public void TestReverseSelectionThreeCircles()
        {
            OsuHitObject[] objects = null!;
            bool[] newCombos = null!;

            AddStep("Add circles", () =>
            {
                var circle1 = new HitCircle
                {
                    StartTime = 0,
                    Position = new Vector2(208, 240)
                };
                var circle2 = new HitCircle
                {
                    StartTime = 200,
                    Position = new Vector2(256, 144)
                };
                var circle3 = new HitCircle
                {
                    StartTime = 400,
                    Position = new Vector2(304, 240)
                };

                EditorBeatmap.AddRange([circle1, circle2, circle3]);
            });

            AddStep("store objects & new combo data", () =>
            {
                objects = getObjects().ToArray();
                newCombos = getObjectNewCombos().ToArray();
            });

            AddStep("Select circles", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));

            AddStep("Reverse selection", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.G);
                InputManager.ReleaseKey(Key.LControl);
            });

            AddAssert("objects reversed", getObjects, () => Is.EqualTo(objects.Reverse()));
            AddAssert("new combo positions preserved", getObjectNewCombos, () => Is.EqualTo(newCombos));
        }

        [Test]
        public void TestReverseSelectionCircleAndSlider()
        {
            OsuHitObject[] objects = null!;
            bool[] newCombos = null!;

            Vector2 sliderHeadOldPosition = default;
            Vector2 sliderTailOldPosition = default;

            AddStep("Add objects", () =>
            {
                var circle = new HitCircle
                {
                    StartTime = 0,
                    Position = new Vector2(208, 240)
                };
                var slider = new Slider
                {
                    StartTime = 200,
                    Position = sliderHeadOldPosition = new Vector2(257, 144),
                    Path = new SliderPath
                    {
                        ControlPoints =
                        {
                            new PathControlPoint(),
                            new PathControlPoint(new Vector2(100))
                        }
                    }
                };

                sliderTailOldPosition = slider.EndPosition;

                EditorBeatmap.AddRange([circle, slider]);
            });

            AddStep("store objects & new combo data", () =>
            {
                objects = getObjects().ToArray();
                newCombos = getObjectNewCombos().ToArray();
            });

            AddStep("Select objects", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));

            AddStep("Reverse selection", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.G);
                InputManager.ReleaseKey(Key.LControl);
            });

            AddAssert("objects reversed", getObjects, () => Is.EqualTo(objects.Reverse()));
            AddAssert("new combo positions preserved", getObjectNewCombos, () => Is.EqualTo(newCombos));

            AddAssert("Slider head is at slider tail", () =>
                Vector2.Distance(EditorBeatmap.HitObjects.OfType<Slider>().ElementAt(0).Position, sliderTailOldPosition) < 1);

            AddAssert("Slider tail is at slider head", () =>
                Vector2.Distance(EditorBeatmap.HitObjects.OfType<Slider>().ElementAt(0).EndPosition, sliderHeadOldPosition) < 1);
        }

        [Test]
        public void TestReverseSelectionTwoCirclesAndSlider()
        {
            OsuHitObject[] objects = null!;
            bool[] newCombos = null!;

            Vector2 sliderHeadOldPosition = default;
            Vector2 sliderTailOldPosition = default;

            AddStep("Add objects", () =>
            {
                var circle1 = new HitCircle
                {
                    StartTime = 0,
                    Position = new Vector2(208, 240)
                };
                var circle2 = new HitCircle
                {
                    StartTime = 200,
                    Position = new Vector2(256, 144)
                };
                var slider = new Slider
                {
                    StartTime = 200,
                    Position = sliderHeadOldPosition = new Vector2(304, 240),
                    Path = new SliderPath
                    {
                        ControlPoints =
                        {
                            new PathControlPoint(),
                            new PathControlPoint(new Vector2(100))
                        }
                    }
                };

                sliderTailOldPosition = slider.EndPosition;

                EditorBeatmap.AddRange([circle1, circle2, slider]);
            });

            AddStep("store objects & new combo data", () =>
            {
                objects = getObjects().ToArray();
                newCombos = getObjectNewCombos().ToArray();
            });

            AddStep("Select objects", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));

            AddStep("Reverse selection", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.G);
                InputManager.ReleaseKey(Key.LControl);
            });

            AddAssert("objects reversed", getObjects, () => Is.EqualTo(objects.Reverse()));
            AddAssert("new combo positions preserved", getObjectNewCombos, () => Is.EqualTo(newCombos));

            AddAssert("Slider head is at slider tail", () =>
                Vector2.Distance(EditorBeatmap.HitObjects.OfType<Slider>().ElementAt(0).Position, sliderTailOldPosition) < 1);

            AddAssert("Slider tail is at slider head", () =>
                Vector2.Distance(EditorBeatmap.HitObjects.OfType<Slider>().ElementAt(0).EndPosition, sliderHeadOldPosition) < 1);
        }

        [Test]
        public void TestReverseSelectionTwoCombos()
        {
            OsuHitObject[] objects = null!;
            bool[] newCombos = null!;

            AddStep("Add circles", () =>
            {
                var circle1 = new HitCircle
                {
                    StartTime = 0,
                    Position = new Vector2(216, 240)
                };
                var circle2 = new HitCircle
                {
                    StartTime = 200,
                    Position = new Vector2(120, 192)
                };
                var circle3 = new HitCircle
                {
                    StartTime = 400,
                    Position = new Vector2(216, 144)
                };

                var circle4 = new HitCircle
                {
                    StartTime = 646,
                    NewCombo = true,
                    Position = new Vector2(296, 240)
                };
                var circle5 = new HitCircle
                {
                    StartTime = 846,
                    Position = new Vector2(392, 162)
                };
                var circle6 = new HitCircle
                {
                    StartTime = 1046,
                    Position = new Vector2(296, 144)
                };

                EditorBeatmap.AddRange([circle1, circle2, circle3, circle4, circle5, circle6]);
            });

            AddStep("store objects & new combo data", () =>
            {
                objects = getObjects().ToArray();
                newCombos = getObjectNewCombos().ToArray();
            });

            AddStep("Select circles", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));

            AddStep("Reverse selection", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.G);
                InputManager.ReleaseKey(Key.LControl);
            });

            AddAssert("objects reversed", getObjects, () => Is.EqualTo(objects.Reverse()));
            AddAssert("new combo positions preserved", getObjectNewCombos, () => Is.EqualTo(newCombos));
        }

        private IEnumerable<OsuHitObject> getObjects() => EditorBeatmap.HitObjects.OfType<OsuHitObject>();

        private IEnumerable<bool> getObjectNewCombos() => getObjects().Select(ho => ho.NewCombo);
    }
}
