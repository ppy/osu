// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
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
    public class TestSceneSliderControlPointPiece : SelectionBlueprintTestScene
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
                    new PathControlPoint(Vector2.Zero, PathType.PerfectCurve),
                    new PathControlPoint(new Vector2(150, 150)),
                    new PathControlPoint(new Vector2(300, 0), PathType.PerfectCurve),
                    new PathControlPoint(new Vector2(400, 0)),
                    new PathControlPoint(new Vector2(400, 150))
                })
            };

            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 2 });

            Add(drawableObject = new DrawableSlider(slider));
            AddBlueprint(new TestSliderBlueprint(slider), drawableObject);
        });

        [Test]
        public void TestDragControlPoint()
        {
            moveMouseToControlPoint(1);
            AddStep("hold", () => InputManager.PressButton(MouseButton.Left));

            addMovementStep(new Vector2(150, 50));
            AddStep("release", () => InputManager.ReleaseButton(MouseButton.Left));

            assertControlPointPosition(1, new Vector2(150, 50));
            assertControlPointType(0, PathType.PerfectCurve);
        }

        [Test]
        public void TestDragControlPointAlmostLinearlyExterior()
        {
            moveMouseToControlPoint(1);
            AddStep("hold", () => InputManager.PressButton(MouseButton.Left));

            addMovementStep(new Vector2(400, 0.01f));
            AddStep("release", () => InputManager.ReleaseButton(MouseButton.Left));

            assertControlPointPosition(1, new Vector2(400, 0.01f));
            assertControlPointType(0, PathType.Bezier);
        }

        [Test]
        public void TestDragControlPointPathRecovery()
        {
            moveMouseToControlPoint(1);
            AddStep("hold", () => InputManager.PressButton(MouseButton.Left));

            addMovementStep(new Vector2(400, 0.01f));
            assertControlPointType(0, PathType.Bezier);

            addMovementStep(new Vector2(150, 50));
            AddStep("release", () => InputManager.ReleaseButton(MouseButton.Left));

            assertControlPointPosition(1, new Vector2(150, 50));
            assertControlPointType(0, PathType.PerfectCurve);
        }

        [Test]
        public void TestDragControlPointPathRecoveryOtherSegment()
        {
            moveMouseToControlPoint(4);
            AddStep("hold", () => InputManager.PressButton(MouseButton.Left));

            addMovementStep(new Vector2(350, 0.01f));
            assertControlPointType(2, PathType.Bezier);

            addMovementStep(new Vector2(150, 150));
            AddStep("release", () => InputManager.ReleaseButton(MouseButton.Left));

            assertControlPointPosition(4, new Vector2(150, 150));
            assertControlPointType(2, PathType.PerfectCurve);
        }

        [Test]
        public void TestDragControlPointPathAfterChangingType()
        {
            AddStep("change type to bezier", () => slider.Path.ControlPoints[2].Type.Value = PathType.Bezier);
            AddStep("add point", () => slider.Path.ControlPoints.Add(new PathControlPoint(new Vector2(500, 10))));
            AddStep("change type to perfect", () => slider.Path.ControlPoints[3].Type.Value = PathType.PerfectCurve);

            moveMouseToControlPoint(4);
            AddStep("hold", () => InputManager.PressButton(MouseButton.Left));

            assertControlPointType(3, PathType.PerfectCurve);

            addMovementStep(new Vector2(350, 0.01f));
            AddStep("release", () => InputManager.ReleaseButton(MouseButton.Left));

            assertControlPointPosition(4, new Vector2(350, 0.01f));
            assertControlPointType(3, PathType.Bezier);
        }

        private void addMovementStep(Vector2 relativePosition)
        {
            AddStep($"move mouse to {relativePosition}", () =>
            {
                Vector2 position = slider.Position + relativePosition;
                InputManager.MoveMouseTo(drawableObject.Parent.ToScreenSpace(position));
            });
        }

        private void moveMouseToControlPoint(int index)
        {
            AddStep($"move mouse to control point {index}", () =>
            {
                Vector2 position = slider.Position + slider.Path.ControlPoints[index].Position.Value;
                InputManager.MoveMouseTo(drawableObject.Parent.ToScreenSpace(position));
            });
        }

        private void assertControlPointType(int index, PathType type) => AddAssert($"control point {index} is {type}", () => slider.Path.ControlPoints[index].Type.Value == type);

        private void assertControlPointPosition(int index, Vector2 position) =>
            AddAssert($"control point {index} at {position}", () => Precision.AlmostEquals(position, slider.Path.ControlPoints[index].Position.Value, 1));

        private class TestSliderBlueprint : SliderSelectionBlueprint
        {
            public new SliderBodyPiece BodyPiece => base.BodyPiece;
            public new TestSliderCircleOverlay HeadOverlay => (TestSliderCircleOverlay)base.HeadOverlay;
            public new TestSliderCircleOverlay TailOverlay => (TestSliderCircleOverlay)base.TailOverlay;
            public new PathControlPointVisualiser ControlPointVisualiser => base.ControlPointVisualiser;

            public TestSliderBlueprint(Slider slider)
                : base(slider)
            {
            }

            protected override SliderCircleOverlay CreateCircleOverlay(Slider slider, SliderPosition position) => new TestSliderCircleOverlay(slider, position);
        }

        private class TestSliderCircleOverlay : SliderCircleOverlay
        {
            public new HitCirclePiece CirclePiece => base.CirclePiece;

            public TestSliderCircleOverlay(Slider slider, SliderPosition position)
                : base(slider, position)
            {
            }
        }
    }
}
