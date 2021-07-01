// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public class TestSceneOsuDistanceSnapGrid : OsuManualInputManagerTestScene
    {
        private const double beat_length = 100;
        private static readonly Vector2 grid_position = new Vector2(512, 384);

        [Cached(typeof(EditorBeatmap))]
        private readonly EditorBeatmap editorBeatmap;

        [Cached]
        private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();

        [Cached(typeof(IPositionSnapProvider))]
        private readonly SnapProvider snapProvider = new SnapProvider();

        private TestOsuDistanceSnapGrid grid;

        public TestSceneOsuDistanceSnapGrid()
        {
            editorBeatmap = new EditorBeatmap(new OsuBeatmap());
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            editorBeatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier = 1;
            editorBeatmap.ControlPointInfo.Clear();
            editorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = beat_length });

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.SlateGray
                },
                grid = new TestOsuDistanceSnapGrid(new HitCircle { Position = grid_position }),
                new SnappingCursorContainer { GetSnapPosition = v => grid.GetSnappedPosition(grid.ToLocalSpace(v)).position }
            };
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
        }

        [Test]
        public void TestCursorInCentre()
        {
            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(grid.ToScreenSpace(grid_position)));
            assertSnappedDistance((float)beat_length);
        }

        [Test]
        public void TestCursorBeforeMovementPoint()
        {
            AddStep("move mouse to just before movement point", () => InputManager.MoveMouseTo(grid.ToScreenSpace(grid_position + new Vector2((float)beat_length, 0) * 1.49f)));
            assertSnappedDistance((float)beat_length);
        }

        [Test]
        public void TestCursorAfterMovementPoint()
        {
            AddStep("move mouse to just after movement point", () => InputManager.MoveMouseTo(grid.ToScreenSpace(grid_position + new Vector2((float)beat_length, 0) * 1.51f)));
            assertSnappedDistance((float)beat_length * 2);
        }

        [Test]
        public void TestLimitedDistance()
        {
            AddStep("create limited grid", () =>
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.SlateGray
                    },
                    grid = new TestOsuDistanceSnapGrid(new HitCircle { Position = grid_position }, new HitCircle { StartTime = 200 }),
                    new SnappingCursorContainer { GetSnapPosition = v => grid.GetSnappedPosition(grid.ToLocalSpace(v)).position }
                };
            });

            AddStep("move mouse outside grid", () => InputManager.MoveMouseTo(grid.ToScreenSpace(grid_position + new Vector2((float)beat_length, 0) * 3f)));
            assertSnappedDistance((float)beat_length * 2);
        }

        private void assertSnappedDistance(float expectedDistance) => AddAssert($"snap distance = {expectedDistance}", () =>
        {
            Vector2 snappedPosition = grid.GetSnappedPosition(grid.ToLocalSpace(InputManager.CurrentState.Mouse.Position)).position;

            return Precision.AlmostEquals(expectedDistance, Vector2.Distance(snappedPosition, grid_position));
        });

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

        private class TestOsuDistanceSnapGrid : OsuDistanceSnapGrid
        {
            public new float DistanceSpacing => base.DistanceSpacing;

            public TestOsuDistanceSnapGrid(OsuHitObject hitObject, OsuHitObject nextHitObject = null)
                : base(hitObject, nextHitObject)
            {
            }
        }

        private class SnapProvider : IPositionSnapProvider
        {
            public SnapResult SnapScreenSpacePositionToValidPosition(Vector2 screenSpacePosition) =>
                new SnapResult(screenSpacePosition, null);

            public SnapResult SnapScreenSpacePositionToValidTime(Vector2 screenSpacePosition) => new SnapResult(screenSpacePosition, 0);

            public float GetBeatSnapDistanceAt(double referenceTime) => (float)beat_length;

            public float DurationToDistance(double referenceTime, double duration) => (float)duration;

            public double DistanceToDuration(double referenceTime, float distance) => distance;

            public double GetSnappedDurationFromDistance(double referenceTime, float distance) => 0;

            public float GetSnappedDistanceFromDistance(double referenceTime, float distance) => 0;
        }
    }
}
