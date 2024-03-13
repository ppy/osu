// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestSceneSliderControlPointPiece : SelectionBlueprintTestScene
    {
        private Slider slider;
        private DrawableSlider drawableObject;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Clear();

            slider = new Slider
            {
                Position = new Vector2(256, 192),
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(Vector2.Zero, PathType.PERFECT_CURVE),
                    new PathControlPoint(new Vector2(150, 150)),
                    new PathControlPoint(new Vector2(300, 0), PathType.PERFECT_CURVE),
                    new PathControlPoint(new Vector2(400, 0)),
                    new PathControlPoint(new Vector2(400, 150))
                })
            };

            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 2 });

            Add(drawableObject = new DrawableSlider(slider));
            AddBlueprint(new TestSliderBlueprint(slider), drawableObject);
        });

        [Test]
        public void TestSelection()
        {
            moveMouseToControlPoint(0);
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));
            assertSelectionCount(1);
            assertSelected(0);

            AddStep("click right mouse", () => InputManager.Click(MouseButton.Right));
            assertSelectionCount(1);
            assertSelected(0);

            moveMouseToControlPoint(3);
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));
            assertSelectionCount(1);
            assertSelected(3);

            AddStep("press control", () => InputManager.PressKey(Key.ControlLeft));
            moveMouseToControlPoint(2);
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));
            assertSelectionCount(2);
            assertSelected(2);
            assertSelected(3);

            moveMouseToControlPoint(0);
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));
            assertSelectionCount(3);
            assertSelected(0);
            assertSelected(2);
            assertSelected(3);

            AddStep("click right mouse", () => InputManager.Click(MouseButton.Right));
            assertSelectionCount(3);
            assertSelected(0);
            assertSelected(2);
            assertSelected(3);

            AddStep("release control", () => InputManager.ReleaseKey(Key.ControlLeft));
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));
            assertSelectionCount(1);
            assertSelected(0);

            moveMouseToRelativePosition(new Vector2(350, 0));
            AddStep("ctrl+click to create new point", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.PressButton(MouseButton.Left);
            });
            assertSelectionCount(1);
            assertSelected(3);

            AddStep("release ctrl+click", () =>
            {
                InputManager.ReleaseButton(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            assertSelectionCount(1);
            assertSelected(3);
        }

        [Test]
        public void TestNewControlPointCreation()
        {
            moveMouseToRelativePosition(new Vector2(350, 0));
            AddStep("ctrl+click to create new point", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.PressButton(MouseButton.Left);
            });
            AddAssert("slider has 6 control points", () => slider.Path.ControlPoints.Count == 6);
            AddStep("release ctrl+click", () =>
            {
                InputManager.ReleaseButton(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            // ensure that the next drag doesn't attempt to move the placement that just finished.
            moveMouseToRelativePosition(new Vector2(0, 50));
            AddStep("press left mouse", () => InputManager.PressButton(MouseButton.Left));
            moveMouseToRelativePosition(new Vector2(0, 100));
            AddStep("release left mouse", () => InputManager.ReleaseButton(MouseButton.Left));
            assertControlPointPosition(3, new Vector2(350, 0));

            moveMouseToRelativePosition(new Vector2(400, 75));
            AddStep("ctrl+click to create new point", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.PressButton(MouseButton.Left);
            });
            AddAssert("slider has 7 control points", () => slider.Path.ControlPoints.Count == 7);
            moveMouseToRelativePosition(new Vector2(350, 75));
            AddStep("release ctrl+click", () =>
            {
                InputManager.ReleaseButton(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            assertControlPointPosition(5, new Vector2(350, 75));

            // ensure that the next drag doesn't attempt to move the placement that just finished.
            moveMouseToRelativePosition(new Vector2(0, 50));
            AddStep("press left mouse", () => InputManager.PressButton(MouseButton.Left));
            moveMouseToRelativePosition(new Vector2(0, 100));
            AddStep("release left mouse", () => InputManager.ReleaseButton(MouseButton.Left));
            assertControlPointPosition(5, new Vector2(350, 75));
        }

        private void assertSelectionCount(int count) =>
            AddAssert($"{count} control point pieces selected", () => this.ChildrenOfType<PathControlPointPiece<Slider>>().Count(piece => piece.IsSelected.Value) == count);

        private void assertSelected(int index) =>
            AddAssert($"{(index + 1).ToOrdinalWords()} control point piece selected",
                () => this.ChildrenOfType<PathControlPointPiece<Slider>>().Single(piece => piece.ControlPoint == slider.Path.ControlPoints[index]).IsSelected.Value);

        private void moveMouseToRelativePosition(Vector2 relativePosition) =>
            AddStep($"move mouse to {relativePosition}", () =>
            {
                Vector2 position = slider.Position + relativePosition;
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(position));
            });

        [Test]
        public void TestDragControlPoint()
        {
            moveMouseToControlPoint(1);
            AddStep("hold", () => InputManager.PressButton(MouseButton.Left));

            addMovementStep(new Vector2(150, 50));
            AddStep("release", () => InputManager.ReleaseButton(MouseButton.Left));

            assertControlPointPosition(1, new Vector2(150, 50));
            assertControlPointType(0, PathType.PERFECT_CURVE);
        }

        [Test]
        public void TestDragMultipleControlPoints()
        {
            moveMouseToControlPoint(2);
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));

            AddStep("hold control", () => InputManager.PressKey(Key.LControl));

            moveMouseToControlPoint(3);
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));

            moveMouseToControlPoint(4);
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));

            moveMouseToControlPoint(2);
            AddStep("hold left mouse", () => InputManager.PressButton(MouseButton.Left));

            AddAssert("three control point pieces selected", () => this.ChildrenOfType<PathControlPointPiece<Slider>>().Count(piece => piece.IsSelected.Value) == 3);

            addMovementStep(new Vector2(450, 50));
            AddStep("release left mouse", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("three control point pieces selected", () => this.ChildrenOfType<PathControlPointPiece<Slider>>().Count(piece => piece.IsSelected.Value) == 3);

            assertControlPointPosition(2, new Vector2(450, 50));
            assertControlPointType(2, PathType.PERFECT_CURVE);

            assertControlPointPosition(3, new Vector2(550, 50));

            assertControlPointPosition(4, new Vector2(550, 200));

            AddStep("release control", () => InputManager.ReleaseKey(Key.LControl));
        }

        [Test]
        public void TestDragMultipleControlPointsIncludingHead()
        {
            moveMouseToControlPoint(0);
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));

            AddStep("hold control", () => InputManager.PressKey(Key.LControl));

            moveMouseToControlPoint(3);
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));

            moveMouseToControlPoint(4);
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));

            moveMouseToControlPoint(3);
            AddStep("hold left mouse", () => InputManager.PressButton(MouseButton.Left));

            AddAssert("three control point pieces selected", () => this.ChildrenOfType<PathControlPointPiece<Slider>>().Count(piece => piece.IsSelected.Value) == 3);

            addMovementStep(new Vector2(550, 50));
            AddStep("release left mouse", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("three control point pieces selected", () => this.ChildrenOfType<PathControlPointPiece<Slider>>().Count(piece => piece.IsSelected.Value) == 3);

            // note: if the head is part of the selection being moved, the entire slider is moved.
            // the unselected nodes will therefore change position relative to the slider head.

            AddAssert("slider moved", () => Precision.AlmostEquals(slider.Position, new Vector2(256, 192) + new Vector2(150, 50)));

            assertControlPointPosition(0, Vector2.Zero);
            assertControlPointType(0, PathType.PERFECT_CURVE);

            assertControlPointPosition(1, new Vector2(0, 100));

            assertControlPointPosition(2, new Vector2(150, -50));

            assertControlPointPosition(3, new Vector2(400, 0));

            assertControlPointPosition(4, new Vector2(400, 150));

            AddStep("release control", () => InputManager.ReleaseKey(Key.LControl));
        }

        [Test]
        public void TestDragControlPointAlmostLinearlyExterior()
        {
            moveMouseToControlPoint(1);
            AddStep("hold", () => InputManager.PressButton(MouseButton.Left));

            addMovementStep(new Vector2(400, 0.01f));
            AddStep("release", () => InputManager.ReleaseButton(MouseButton.Left));

            assertControlPointPosition(1, new Vector2(400, 0.01f));
            assertControlPointType(0, PathType.BEZIER);
        }

        [Test]
        public void TestDragControlPointPathRecovery()
        {
            moveMouseToControlPoint(1);
            AddStep("hold", () => InputManager.PressButton(MouseButton.Left));

            addMovementStep(new Vector2(400, 0.01f));
            assertControlPointType(0, PathType.BEZIER);

            addMovementStep(new Vector2(150, 50));
            AddStep("release", () => InputManager.ReleaseButton(MouseButton.Left));

            assertControlPointPosition(1, new Vector2(150, 50));
            assertControlPointType(0, PathType.PERFECT_CURVE);
        }

        [Test]
        public void TestDragControlPointPathRecoveryOtherSegment()
        {
            moveMouseToControlPoint(4);
            AddStep("hold", () => InputManager.PressButton(MouseButton.Left));

            addMovementStep(new Vector2(350, 0.01f));
            assertControlPointType(2, PathType.BEZIER);

            addMovementStep(new Vector2(150, 150));
            AddStep("release", () => InputManager.ReleaseButton(MouseButton.Left));

            assertControlPointPosition(4, new Vector2(150, 150));
            assertControlPointType(2, PathType.PERFECT_CURVE);
        }

        [Test]
        public void TestDragControlPointPathAfterChangingType()
        {
            AddStep("change type to bezier", () => slider.Path.ControlPoints[2].Type = PathType.BEZIER);
            AddStep("add point", () => slider.Path.ControlPoints.Add(new PathControlPoint(new Vector2(500, 10))));
            AddStep("change type to perfect", () => slider.Path.ControlPoints[3].Type = PathType.PERFECT_CURVE);

            moveMouseToControlPoint(4);
            AddStep("hold", () => InputManager.PressButton(MouseButton.Left));

            assertControlPointType(3, PathType.PERFECT_CURVE);

            addMovementStep(new Vector2(350, 0.01f));
            AddStep("release", () => InputManager.ReleaseButton(MouseButton.Left));

            assertControlPointPosition(4, new Vector2(350, 0.01f));
            assertControlPointType(3, PathType.BEZIER);
        }

        private void addMovementStep(Vector2 relativePosition)
        {
            AddStep($"move mouse to {relativePosition}", () =>
            {
                Vector2 position = slider.Position + relativePosition;
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(position));
            });
        }

        private void moveMouseToControlPoint(int index)
        {
            AddStep($"move mouse to control point {index}", () =>
            {
                Vector2 position = slider.Position + slider.Path.ControlPoints[index].Position;
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(position));
            });
        }

        private void assertControlPointType(int index, PathType type) => AddAssert($"control point {index} is {type}", () => slider.Path.ControlPoints[index].Type == type);

        private void assertControlPointPosition(int index, Vector2 position) =>
            AddAssert($"control point {index} at {position}", () => Precision.AlmostEquals(position, slider.Path.ControlPoints[index].Position, 1));

        private partial class TestSliderBlueprint : SliderSelectionBlueprint
        {
            public new SliderBodyPiece BodyPiece => base.BodyPiece;
            public new TestSliderCircleOverlay HeadOverlay => (TestSliderCircleOverlay)base.HeadOverlay;
            public new TestSliderCircleOverlay TailOverlay => (TestSliderCircleOverlay)base.TailOverlay;
            public new PathControlPointVisualiser<Slider> ControlPointVisualiser => base.ControlPointVisualiser;

            public TestSliderBlueprint(Slider slider)
                : base(slider)
            {
            }

            protected override SliderCircleOverlay CreateCircleOverlay(Slider slider, SliderPosition position) => new TestSliderCircleOverlay(slider, position);
        }

        private partial class TestSliderCircleOverlay : SliderCircleOverlay
        {
            public new HitCirclePiece CirclePiece => base.CirclePiece;

            public TestSliderCircleOverlay(Slider slider, SliderPosition position)
                : base(slider, position)
            {
            }
        }
    }
}
