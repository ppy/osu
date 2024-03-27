// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
            Vector2 circle1OldPosition = default;
            Vector2 circle2OldPosition = default;

            AddStep("Add circles", () =>
            {
                var circle1 = new HitCircle
                {
                    StartTime = 0,
                    Position = circle1OldPosition = new Vector2(208, 240)
                };
                var circle2 = new HitCircle
                {
                    StartTime = 200,
                    Position = circle2OldPosition = new Vector2(256, 144)
                };

                EditorBeatmap.AddRange([circle1, circle2]);
            });

            AddStep("Select circles", () =>
            {
                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects);
            });

            AddStep("Reverse selection", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.G);
                InputManager.ReleaseKey(Key.LControl);
            });

            AddAssert("circle1 is at circle2 position",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(0).Position,
                () => Is.EqualTo(circle2OldPosition)
            );

            AddAssert("circle2 is at circle1 position",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(1).Position,
                () => Is.EqualTo(circle1OldPosition)
            );

            AddAssert("circle2 is not a new combo",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(1).NewCombo,
                () => Is.EqualTo(false)
            );
        }

        [Test]
        public void TestReverseSelectionThreeCircles()
        {
            Vector2 circle1OldPosition = default;
            Vector2 circle2OldPosition = default;
            Vector2 circle3OldPosition = default;

            AddStep("Add circles", () =>
            {
                var circle1 = new HitCircle
                {
                    StartTime = 0,
                    Position = circle1OldPosition = new Vector2(208, 240)
                };
                var circle2 = new HitCircle
                {
                    StartTime = 200,
                    Position = circle2OldPosition = new Vector2(256, 144)
                };
                var circle3 = new HitCircle
                {
                    StartTime = 400,
                    Position = circle3OldPosition = new Vector2(304, 240)
                };

                EditorBeatmap.AddRange([circle1, circle2, circle3]);
            });

            AddStep("Select circles", () =>
            {
                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects);
            });

            AddStep("Reverse selection", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.G);
                InputManager.ReleaseKey(Key.LControl);
            });

            AddAssert("circle1 is at circle3 position",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(0).Position,
                () => Is.EqualTo(circle3OldPosition)
            );

            AddAssert("circle3 is at circle1 position",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(2).Position,
                () => Is.EqualTo(circle1OldPosition)
            );

            AddAssert("circle3 is not a new combo",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(2).NewCombo,
                () => Is.EqualTo(false)
            );
        }

        [Test]
        public void TestReverseSelectionCircleAndSlider()
        {
            Vector2 circleOldPosition = default;
            Vector2 sliderHeadOldPosition = default;
            Vector2 sliderTailOldPosition = default;

            AddStep("Add objects", () =>
            {
                var circle = new HitCircle
                {
                    StartTime = 0,
                    Position = circleOldPosition = new Vector2(208, 240)
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

            AddStep("Select objects", () =>
            {
                var circle = (HitCircle)EditorBeatmap.HitObjects[0];
                var slider = (Slider)EditorBeatmap.HitObjects[1];

                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects);
            });

            AddStep("Reverse selection", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.G);
                InputManager.ReleaseKey(Key.LControl);
            });

            AddAssert("circle is at the same position",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(0).Position,
                () => Is.EqualTo(circleOldPosition)
            );

            AddAssert("Slider head is at slider tail", () =>
                Vector2.Distance(EditorBeatmap.HitObjects.OfType<Slider>().ElementAt(0).Position, sliderTailOldPosition) < 1);

            AddAssert("Slider tail is at slider head", () =>
                Vector2.Distance(EditorBeatmap.HitObjects.OfType<Slider>().ElementAt(0).EndPosition, sliderHeadOldPosition) < 1);
        }

        [Test]
        public void TestReverseSelectionTwoCirclesAndSlider()
        {
            Vector2 circle1OldPosition = default;
            Vector2 circle2OldPosition = default;

            Vector2 sliderHeadOldPosition = default;
            Vector2 sliderTailOldPosition = default;

            AddStep("Add objects", () =>
            {
                var circle1 = new HitCircle
                {
                    StartTime = 0,
                    Position = circle1OldPosition = new Vector2(208, 240)
                };
                var circle2 = new HitCircle
                {
                    StartTime = 200,
                    Position = circle2OldPosition = new Vector2(256, 144)
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

            AddStep("Select objects", () =>
            {
                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects);
            });

            AddStep("Reverse selection", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.G);
                InputManager.ReleaseKey(Key.LControl);
            });

            AddAssert("circle1 is at circle2 position",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(0).Position,
                () => Is.EqualTo(circle2OldPosition)
            );

            AddAssert("circle2 is at circle1 position",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(1).Position,
                () => Is.EqualTo(circle1OldPosition)
            );

            AddAssert("Slider head is at slider tail", () =>
                Vector2.Distance(EditorBeatmap.HitObjects.OfType<Slider>().ElementAt(0).Position, sliderTailOldPosition) < 1);

            AddAssert("Slider tail is at slider head", () =>
                Vector2.Distance(EditorBeatmap.HitObjects.OfType<Slider>().ElementAt(0).EndPosition, sliderHeadOldPosition) < 1);
        }

        [Test]
        public void TestReverseSelectionTwoCombos()
        {
            Vector2 circle1OldPosition = default;
            Vector2 circle2OldPosition = default;
            Vector2 circle3OldPosition = default;

            Vector2 circle4OldPosition = default;
            Vector2 circle5OldPosition = default;
            Vector2 circle6OldPosition = default;

            AddStep("Add circles", () =>
            {
                var circle1 = new HitCircle
                {
                    StartTime = 0,
                    Position = circle1OldPosition = new Vector2(216, 240)
                };
                var circle2 = new HitCircle
                {
                    StartTime = 200,
                    Position = circle2OldPosition = new Vector2(120, 192)
                };
                var circle3 = new HitCircle
                {
                    StartTime = 400,
                    Position = circle3OldPosition = new Vector2(216, 144)
                };

                var circle4 = new HitCircle
                {
                    StartTime = 646,
                    NewCombo = true,
                    Position = circle4OldPosition = new Vector2(296, 240)
                };
                var circle5 = new HitCircle
                {
                    StartTime = 846,
                    Position = circle5OldPosition = new Vector2(392, 162)
                };
                var circle6 = new HitCircle
                {
                    StartTime = 1046,
                    Position = circle6OldPosition = new Vector2(296, 144)
                };

                EditorBeatmap.AddRange([circle1, circle2, circle3, circle4, circle5, circle6]);
            });

            AddStep("Select circles", () =>
            {
                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects);
            });

            AddStep("Reverse selection", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.G);
                InputManager.ReleaseKey(Key.LControl);
            });

            AddAssert("circle1 is at circle6 position",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(0).Position,
                () => Is.EqualTo(circle6OldPosition)
            );

            AddAssert("circle2 is at circle5 position",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(1).Position,
                () => Is.EqualTo(circle5OldPosition)
            );

            AddAssert("circle3 is at circle4 position",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(2).Position,
                () => Is.EqualTo(circle4OldPosition)
            );

            AddAssert("circle4 is at circle3 position",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(3).Position,
                () => Is.EqualTo(circle3OldPosition)
            );

            AddAssert("circle5 is at circle2 position",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(4).Position,
                () => Is.EqualTo(circle2OldPosition)
            );

            AddAssert("circle6 is at circle1 position",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(5).Position,
                () => Is.EqualTo(circle1OldPosition)
            );
        }
    }
}
