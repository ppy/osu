// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneOsuBeatSnapGrid : ManualInputManagerTestScene
    {
        private const double beat_length = 100;
        private static readonly Vector2 grid_position = new Vector2(512, 384);

        [Cached(typeof(IEditorBeatmap))]
        private readonly EditorBeatmap<OsuHitObject> editorBeatmap;

        [Cached]
        private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();

        private TestOsuBeatSnapGrid grid;

        public TestSceneOsuBeatSnapGrid()
        {
            editorBeatmap = new EditorBeatmap<OsuHitObject>(new OsuBeatmap());

            createGrid();
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Clear();

            editorBeatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier = 1;
            editorBeatmap.ControlPointInfo.DifficultyPoints.Clear();
            editorBeatmap.ControlPointInfo.TimingPoints.Clear();
            editorBeatmap.ControlPointInfo.TimingPoints.Add(new TimingControlPoint { BeatLength = beat_length });

            beatDivisor.Value = 1;
        });

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(6)]
        [TestCase(8)]
        [TestCase(12)]
        [TestCase(16)]
        public void TestBeatDivisor(int divisor)
        {
            AddStep($"set beat divisor = {divisor}", () => beatDivisor.Value = divisor);
            createGrid();
        }

        [TestCase(100, 100)]
        [TestCase(200, 100)]
        public void TestBeatLength(float beatLength, float expectedSpacing)
        {
            AddStep($"set beat length = {beatLength}", () =>
            {
                editorBeatmap.ControlPointInfo.TimingPoints.Clear();
                editorBeatmap.ControlPointInfo.TimingPoints.Add(new TimingControlPoint { BeatLength = beatLength });
            });

            createGrid();
            AddAssert($"spacing = {expectedSpacing}", () => Precision.AlmostEquals(expectedSpacing, grid.DistanceSpacing));
        }

        [TestCase(0.5f, 50)]
        [TestCase(1, 100)]
        [TestCase(1.5f, 150)]
        public void TestSpeedMultiplier(float multiplier, float expectedSpacing)
        {
            AddStep($"set speed multiplier = {multiplier}", () =>
            {
                editorBeatmap.ControlPointInfo.DifficultyPoints.Clear();
                editorBeatmap.ControlPointInfo.DifficultyPoints.Add(new DifficultyControlPoint { SpeedMultiplier = multiplier });
            });

            createGrid();
            AddAssert($"spacing = {expectedSpacing}", () => Precision.AlmostEquals(expectedSpacing, grid.DistanceSpacing));
        }

        [TestCase(0.5f, 50)]
        [TestCase(1, 100)]
        [TestCase(1.5f, 150)]
        public void TestSliderMultiplier(float multiplier, float expectedSpacing)
        {
            AddStep($"set speed multiplier = {multiplier}", () => editorBeatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier = multiplier);
            createGrid();
            AddAssert($"spacing = {expectedSpacing}", () => Precision.AlmostEquals(expectedSpacing, grid.DistanceSpacing));
        }

        [Test]
        public void TestCursorInCentre()
        {
            createGrid();

            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(grid.ToScreenSpace(grid_position)));
            assertSnappedDistance((float)beat_length);
        }

        [Test]
        public void TestCursorBeforeMovementPoint()
        {
            createGrid();

            AddStep("move mouse to just before movement point", () => InputManager.MoveMouseTo(grid.ToScreenSpace(grid_position + new Vector2((float)beat_length, 0) * 1.49f)));
            assertSnappedDistance((float)beat_length);
        }

        [Test]
        public void TestCursorAfterMovementPoint()
        {
            createGrid();

            AddStep("move mouse to just after movement point", () => InputManager.MoveMouseTo(grid.ToScreenSpace(grid_position + new Vector2((float)beat_length, 0) * 1.51f)));
            assertSnappedDistance((float)beat_length * 2);
        }

        private void assertSnappedDistance(float expectedDistance) => AddAssert($"snap distance = {expectedDistance}", () =>
        {
            Vector2 snappedPosition = grid.GetSnapPosition(grid.ToLocalSpace(InputManager.CurrentState.Mouse.Position));
            float distance = Vector2.Distance(snappedPosition, grid_position);

            return Precision.AlmostEquals(expectedDistance, distance);
        });

        private void createGrid()
        {
            AddStep("create grid", () =>
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.SlateGray
                    },
                    grid = new TestOsuBeatSnapGrid(new HitCircle { Position = grid_position }),
                    new SnappingCursorContainer { GetSnapPosition = v => grid.GetSnapPosition(grid.ToLocalSpace(v)) }
                };
            });
        }

        private class SnappingCursorContainer : CompositeDrawable
        {
            public Func<Vector2, Vector2> GetSnapPosition;

            private readonly Drawable cursor;

            public SnappingCursorContainer()
            {
                RelativeSizeAxes = Axes.Both;

                InternalChild = cursor = new Circle
                {
                    Origin = Anchor.Centre,
                    Size = new Vector2(50),
                    Colour = Color4.Red
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updatePosition(GetContainingInputManager().CurrentState.Mouse.Position);
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                base.OnMouseMove(e);

                updatePosition(e.ScreenSpaceMousePosition);
                return true;
            }

            private void updatePosition(Vector2 screenSpacePosition)
            {
                cursor.Position = GetSnapPosition.Invoke(screenSpacePosition);
            }
        }

        private class TestOsuBeatSnapGrid : OsuBeatSnapGrid
        {
            public new float DistanceSpacing => base.DistanceSpacing;

            public TestOsuBeatSnapGrid(OsuHitObject hitObject)
                : base(hitObject)
            {
            }
        }

        private abstract class CircularBeatSnapGrid : BeatSnapGrid
        {
            protected CircularBeatSnapGrid(HitObject hitObject, Vector2 centrePosition)
                : base(hitObject, centrePosition)
            {
            }

            protected override void CreateContent(Vector2 centrePosition)
            {
                float maxDistance = Math.Max(
                    Vector2.Distance(centrePosition, Vector2.Zero),
                    Math.Max(
                        Vector2.Distance(centrePosition, new Vector2(DrawWidth, 0)),
                        Math.Max(
                            Vector2.Distance(centrePosition, new Vector2(0, DrawHeight)),
                            Vector2.Distance(centrePosition, DrawSize))));

                int requiredCircles = (int)(maxDistance / DistanceSpacing);

                for (int i = 0; i < requiredCircles; i++)
                {
                    float radius = (i + 1) * DistanceSpacing * 2;

                    AddInternal(new CircularProgress
                    {
                        Origin = Anchor.Centre,
                        Position = centrePosition,
                        Current = { Value = 1 },
                        Size = new Vector2(radius),
                        InnerRadius = 4 * 1f / radius,
                        Colour = GetColourForBeatIndex(i)
                    });
                }
            }

            public override Vector2 GetSnapPosition(Vector2 position)
            {
                Vector2 direction = position - CentrePosition;
                float distance = direction.Length;

                float radius = DistanceSpacing;
                int radialCount = Math.Max(1, (int)Math.Round(distance / radius));

                if (radialCount <= 0)
                    return position;

                Vector2 normalisedDirection = direction * new Vector2(1f / distance);

                return CentrePosition + normalisedDirection * radialCount * radius;
            }
        }

        private class OsuBeatSnapGrid : CircularBeatSnapGrid
        {
            /// <summary>
            /// Scoring distance with a speed-adjusted beat length of 1 second.
            /// </summary>
            private const float base_scoring_distance = 100;

            public OsuBeatSnapGrid(OsuHitObject hitObject)
                : base(hitObject, hitObject.StackedEndPosition)
            {
            }

            protected override float GetVelocity(double time, ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
            {
                TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(time);
                DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(time);

                double scoringDistance = base_scoring_distance * difficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier;

                return (float)(scoringDistance / timingPoint.BeatLength);
            }
        }
    }
}
