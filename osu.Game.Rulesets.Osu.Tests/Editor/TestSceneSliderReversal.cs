// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestSceneSliderReversal : TestSceneOsuEditor
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(Ruleset.Value, false);

        private readonly PathControlPoint[][] paths =
        {
            createPathSegment(
                PathType.PERFECT_CURVE,
                new Vector2(200, -50),
                new Vector2(250, 0)
            ),
            createPathSegment(
                PathType.LINEAR,
                new Vector2(100, 0),
                new Vector2(100, 100)
            ),
            createPathSegment(
                PathType.PERFECT_CURVE,
                new Vector2(100.009f, -50.0009f),
                new Vector2(200.0089f, -100)
            ),
            createPathSegment(
                PathType.PERFECT_CURVE,
                new Vector2(25, -50),
                new Vector2(100, 75)
            )
        };

        private static PathControlPoint[] createPathSegment(PathType type, params Vector2[] positions)
        {
            return positions.Select(p => new PathControlPoint
            {
                Position = p
            }).Prepend(new PathControlPoint
            {
                Type = type
            }).ToArray();
        }

        private Slider selectedSlider => (Slider)EditorBeatmap.SelectedHitObjects[0];

        [TestCase(0, 250)]
        [TestCase(0, 200)]
        [TestCase(1, 120, false, false)]
        [TestCase(1, 80, false, false)]
        [TestCase(2, 250)]
        [TestCase(2, 190)]
        [TestCase(3, 250)]
        [TestCase(3, 190)]
        public void TestSliderReversal(int pathIndex, double length, bool assertEqualDistances = true, bool assertSliderReduction = true)
        {
            var controlPoints = paths[pathIndex];

            Vector2 oldStartPos = default;
            Vector2 oldEndPos = default;
            double oldDistance = default;
            var oldControlPointTypes = controlPoints.Select(p => p.Type);

            AddStep("Add slider", () =>
            {
                var slider = new Slider
                {
                    Position = new Vector2(OsuPlayfield.BASE_SIZE.X / 2, OsuPlayfield.BASE_SIZE.Y / 2),
                    Path = new SliderPath(controlPoints)
                    {
                        ExpectedDistance = { Value = length }
                    }
                };

                EditorBeatmap.Add(slider);

                oldStartPos = slider.Position;
                oldEndPos = slider.EndPosition;
                oldDistance = slider.Path.Distance;
            });

            AddStep("Select slider", () =>
            {
                var slider = (Slider)EditorBeatmap.HitObjects[0];
                EditorBeatmap.SelectedHitObjects.Add(slider);
            });

            AddStep("Reverse slider", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.G);
                InputManager.ReleaseKey(Key.LControl);
            });

            if (pathIndex == 2)
            {
                AddRepeatStep("Reverse slider again", () =>
                {
                    InputManager.PressKey(Key.LControl);
                    InputManager.Key(Key.G);
                    InputManager.ReleaseKey(Key.LControl);
                }, 2);
            }

            if (assertEqualDistances)
            {
                AddAssert("Middle control point has the same distance from start to end", () =>
                {
                    var pathControlPoints = selectedSlider.Path.ControlPoints;
                    float middleToStart = Vector2.Distance(pathControlPoints[^2].Position, pathControlPoints[0].Position);
                    float middleToEnd = Vector2.Distance(pathControlPoints[^2].Position, pathControlPoints[^1].Position);

                    return Precision.AlmostEquals(middleToStart, middleToEnd, 1f);
                });
            }

            AddAssert("Middle control point is not at start or end", () =>
                Vector2.Distance(selectedSlider.Path.ControlPoints[^2].Position, oldStartPos) > 1 &&
                Vector2.Distance(selectedSlider.Path.ControlPoints[^2].Position, oldEndPos) > 1
            );

            AddAssert("Slider has correct length", () =>
                Precision.AlmostEquals(selectedSlider.Path.Distance, oldDistance));

            AddAssert("Slider has correct start position", () =>
                Vector2.Distance(selectedSlider.Position, oldEndPos) < 1);

            AddAssert("Slider has correct end position", () =>
                Vector2.Distance(selectedSlider.EndPosition, oldStartPos) < 1);

            AddAssert("Control points have correct types", () =>
            {
                var newControlPointTypes = selectedSlider.Path.ControlPoints.Select(p => p.Type).ToArray();

                return oldControlPointTypes.Take(newControlPointTypes.Length).SequenceEqual(newControlPointTypes);
            });

            if (assertSliderReduction)
            {
                AddStep("Move to marker", () =>
                {
                    var marker = this.ChildrenOfType<SliderEndDragMarker>().Single();
                    var markerPos = (marker.ScreenSpaceDrawQuad.TopRight + marker.ScreenSpaceDrawQuad.BottomRight) / 2;
                    // sometimes the cursor may miss the marker's hitbox so we
                    // add a little offset here to be sure it lands in a clickable position.
                    var position = new Vector2(markerPos.X + 2f, markerPos.Y);
                    InputManager.MoveMouseTo(position);
                });
                AddStep("Click", () => InputManager.PressButton(MouseButton.Left));
                AddStep("Reduce slider", () =>
                {
                    var middleControlPoint = this.ChildrenOfType<PathControlPointPiece<Slider>>().ToArray()[^2];
                    InputManager.MoveMouseTo(middleControlPoint);
                });
                AddStep("Release click", () => InputManager.ReleaseButton(MouseButton.Left));

                AddStep("Save half slider info", () =>
                {
                    oldStartPos = selectedSlider.Position;
                    oldEndPos = selectedSlider.EndPosition;
                    oldDistance = selectedSlider.Path.Distance;
                });

                AddStep("Reverse slider", () =>
                {
                    InputManager.PressKey(Key.LControl);
                    InputManager.Key(Key.G);
                    InputManager.ReleaseKey(Key.LControl);
                });

                AddAssert("Middle control point has the same distance from start to end", () =>
                {
                    var pathControlPoints = selectedSlider.Path.ControlPoints;
                    float middleToStart = Vector2.Distance(pathControlPoints[^2].Position, pathControlPoints[0].Position);
                    float middleToEnd = Vector2.Distance(pathControlPoints[^2].Position, pathControlPoints[^1].Position);

                    return Precision.AlmostEquals(middleToStart, middleToEnd, 1f);
                });

                AddAssert("Middle control point is not at start or end", () =>
                    Vector2.Distance(selectedSlider.Path.ControlPoints[^2].Position, oldStartPos) > 1 &&
                    Vector2.Distance(selectedSlider.Path.ControlPoints[^2].Position, oldEndPos) > 1
                );

                AddAssert("Slider has correct length", () =>
                    Precision.AlmostEquals(selectedSlider.Path.Distance, oldDistance));

                AddAssert("Slider has correct start position", () =>
                    Vector2.Distance(selectedSlider.Position, oldEndPos) < 1);

                AddAssert("Slider has correct end position", () =>
                    Vector2.Distance(selectedSlider.EndPosition, oldStartPos) < 1);

                AddAssert("Control points have correct types", () =>
                {
                    var newControlPointTypes = selectedSlider.Path.ControlPoints.Select(p => p.Type).ToArray();

                    return oldControlPointTypes.Take(newControlPointTypes.Length).SequenceEqual(newControlPointTypes);
                });
            }
        }

        [Test]
        public void TestSegmentedSliderReversal()
        {
            PathControlPoint[] segmentedSliderPath =
            [
                new PathControlPoint
                {
                    Position = new Vector2(0, 0),
                    Type = PathType.PERFECT_CURVE
                },
                new PathControlPoint
                {
                    Position = new Vector2(100, 150),
                },
                new PathControlPoint
                {
                    Position = new Vector2(75, -50),
                    Type = PathType.PERFECT_CURVE
                },
                new PathControlPoint
                {
                    Position = new Vector2(225, -75),
                },
                new PathControlPoint
                {
                    Position = new Vector2(350, 50),
                    Type = PathType.PERFECT_CURVE
                },
                new PathControlPoint
                {
                    Position = new Vector2(500, -75),
                },
                new PathControlPoint
                {
                    Position = new Vector2(350, -120),
                },
            ];

            Vector2 oldStartPos = default;
            Vector2 oldEndPos = default;
            double oldDistance = default;

            var oldControlPointTypes = segmentedSliderPath.Select(p => p.Type);

            AddStep("Add slider", () =>
            {
                var slider = new Slider
                {
                    Position = new Vector2(0, 200),
                    Path = new SliderPath(segmentedSliderPath)
                    {
                        ExpectedDistance = { Value = 1314 }
                    }
                };

                EditorBeatmap.Add(slider);

                oldStartPos = slider.Position;
                oldEndPos = slider.EndPosition;
                oldDistance = slider.Path.Distance;
            });

            AddStep("Select slider", () =>
            {
                var slider = (Slider)EditorBeatmap.HitObjects[0];
                EditorBeatmap.SelectedHitObjects.Add(slider);
            });

            AddRepeatStep("Reverse slider", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.G);
                InputManager.ReleaseKey(Key.LControl);
            }, 3);

            AddAssert("First arc's control is not at the slider's middle", () =>
                Vector2.Distance(selectedSlider.Path.ControlPoints[^2].Position, selectedSlider.Path.PositionAt(0.5)) > 1
            );

            AddAssert("Last arc's control is not at the slider's middle", () =>
                Vector2.Distance(selectedSlider.Path.ControlPoints[1].Position, selectedSlider.Path.PositionAt(0.5)) > 1
            );

            AddAssert("First arc centered middle control point", () =>
            {
                var pathControlPoints = selectedSlider.Path.ControlPoints;
                float middleToStart = Vector2.Distance(pathControlPoints[1].Position, pathControlPoints[0].Position);
                float middleToEnd = Vector2.Distance(pathControlPoints[1].Position, pathControlPoints[2].Position);

                return Precision.AlmostEquals(middleToStart, middleToEnd, 1f);
            });

            AddAssert("Last arc centered middle control point", () =>
            {
                var pathControlPoints = selectedSlider.Path.ControlPoints;
                float middleToStart = Vector2.Distance(pathControlPoints[^2].Position, pathControlPoints[^3].Position);
                float middleToEnd = Vector2.Distance(pathControlPoints[^2].Position, pathControlPoints[^1].Position);

                return Precision.AlmostEquals(middleToStart, middleToEnd, 1f);
            });

            AddAssert("Slider has correct length", () =>
                Precision.AlmostEquals(selectedSlider.Path.Distance, oldDistance));

            AddAssert("Slider has correct start position", () =>
                Vector2.Distance(selectedSlider.Position, oldEndPos) < 1);

            AddAssert("Slider has correct end position", () =>
                Vector2.Distance(selectedSlider.EndPosition, oldStartPos) < 1);

            AddAssert("Control points have correct types", () =>
            {
                var newControlPointTypes = selectedSlider.Path.ControlPoints.Select(p => p.Type).ToArray();

                return oldControlPointTypes.Take(newControlPointTypes.Length).SequenceEqual(newControlPointTypes);
            });
        }
    }
}
