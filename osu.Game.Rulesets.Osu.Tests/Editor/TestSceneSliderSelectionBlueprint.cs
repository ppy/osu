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
    public class TestSceneSliderSelectionBlueprint : SelectionBlueprintTestScene
    {
        private Slider slider;
        private DrawableSlider drawableObject;
        private TestSliderBlueprint blueprint;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Clear();

            slider = new Slider
            {
                Position = new Vector2(256, 192),
                Path = new SliderPath(PathType.Bezier, new[]
                {
                    Vector2.Zero,
                    new Vector2(150, 150),
                    new Vector2(300, 0)
                })
            };

            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 2 });

            Add(drawableObject = new DrawableSlider(slider));
            AddBlueprint(blueprint = new TestSliderBlueprint(drawableObject));
        });

        [Test]
        public void TestInitialState()
        {
            checkPositions();
        }

        [Test]
        public void TestMoveHitObject()
        {
            moveHitObject();
            checkPositions();
        }

        [Test]
        public void TestMoveAfterApplyingDefaults()
        {
            AddStep("apply defaults", () => slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 2 }));
            moveHitObject();
            checkPositions();
        }

        [Test]
        public void TestStackedHitObject()
        {
            AddStep("set stacking", () => slider.StackHeight = 5);
            checkPositions();
        }

        [Test]
        public void TestSingleControlPointSelection()
        {
            moveMouseToControlPoint(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkControlPointSelected(0, true);
            checkControlPointSelected(1, false);
        }

        [Test]
        public void TestSingleControlPointDeselectionViaOtherControlPoint()
        {
            moveMouseToControlPoint(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));

            moveMouseToControlPoint(1);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkControlPointSelected(0, false);
            checkControlPointSelected(1, true);
        }

        [Test]
        public void TestSingleControlPointDeselectionViaClickOutside()
        {
            moveMouseToControlPoint(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddStep("move mouse outside control point", () => InputManager.MoveMouseTo(drawableObject));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkControlPointSelected(0, false);
            checkControlPointSelected(1, false);
        }

        [Test]
        public void TestMultipleControlPointSelection()
        {
            moveMouseToControlPoint(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            moveMouseToControlPoint(1);
            AddStep("ctrl + click", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            checkControlPointSelected(0, true);
            checkControlPointSelected(1, true);
        }

        [Test]
        public void TestMultipleControlPointDeselectionViaOtherControlPoint()
        {
            moveMouseToControlPoint(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            moveMouseToControlPoint(1);
            AddStep("ctrl + click", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            moveMouseToControlPoint(2);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkControlPointSelected(0, false);
            checkControlPointSelected(1, false);
        }

        [Test]
        public void TestMultipleControlPointDeselectionViaClickOutside()
        {
            moveMouseToControlPoint(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            moveMouseToControlPoint(1);
            AddStep("ctrl + click", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddStep("move mouse outside control point", () => InputManager.MoveMouseTo(drawableObject));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkControlPointSelected(0, false);
            checkControlPointSelected(1, false);
        }

        private void moveHitObject()
        {
            AddStep("move hitobject", () =>
            {
                slider.Position = new Vector2(300, 225);
            });
        }

        private void checkPositions()
        {
            AddAssert("body positioned correctly", () => blueprint.BodyPiece.Position == slider.StackedPosition);

            AddAssert("head positioned correctly",
                () => Precision.AlmostEquals(blueprint.HeadBlueprint.CirclePiece.ScreenSpaceDrawQuad.Centre, drawableObject.HeadCircle.ScreenSpaceDrawQuad.Centre));

            AddAssert("tail positioned correctly",
                () => Precision.AlmostEquals(blueprint.TailBlueprint.CirclePiece.ScreenSpaceDrawQuad.Centre, drawableObject.TailCircle.ScreenSpaceDrawQuad.Centre));
        }

        private void moveMouseToControlPoint(int index)
        {
            AddStep($"move mouse to control point {index}", () =>
            {
                Vector2 position = slider.Position + slider.Path.ControlPoints[index].Position.Value;
                InputManager.MoveMouseTo(drawableObject.Parent.ToScreenSpace(position));
            });
        }

        private void checkControlPointSelected(int index, bool selected)
            => AddAssert($"control point {index} {(selected ? "selected" : "not selected")}", () => blueprint.ControlPointVisualiser.Pieces[index].IsSelected.Value == selected);

        private class TestSliderBlueprint : SliderSelectionBlueprint
        {
            public new SliderBodyPiece BodyPiece => base.BodyPiece;
            public new TestSliderCircleBlueprint HeadBlueprint => (TestSliderCircleBlueprint)base.HeadBlueprint;
            public new TestSliderCircleBlueprint TailBlueprint => (TestSliderCircleBlueprint)base.TailBlueprint;
            public new PathControlPointVisualiser ControlPointVisualiser => base.ControlPointVisualiser;

            public TestSliderBlueprint(DrawableSlider slider)
                : base(slider)
            {
            }

            protected override SliderCircleSelectionBlueprint CreateCircleSelectionBlueprint(DrawableSlider slider, SliderPosition position) => new TestSliderCircleBlueprint(slider, position);
        }

        private class TestSliderCircleBlueprint : SliderCircleSelectionBlueprint
        {
            public new HitCirclePiece CirclePiece => base.CirclePiece;

            public TestSliderCircleBlueprint(DrawableSlider slider, SliderPosition position)
                : base(slider, position)
            {
            }
        }
    }
}
