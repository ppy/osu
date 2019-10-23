// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.MathUtils;
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

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneSliderSelectionBlueprint : SelectionBlueprintTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SliderSelectionBlueprint),
            typeof(SliderCircleSelectionBlueprint),
            typeof(SliderBodyPiece),
            typeof(SliderCircle),
            typeof(PathControlPointVisualiser),
            typeof(PathControlPointPiece)
        };

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

        private class TestSliderBlueprint : SliderSelectionBlueprint
        {
            public new SliderBodyPiece BodyPiece => base.BodyPiece;
            public new TestSliderCircleBlueprint HeadBlueprint => (TestSliderCircleBlueprint)base.HeadBlueprint;
            public new TestSliderCircleBlueprint TailBlueprint => (TestSliderCircleBlueprint)base.TailBlueprint;

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
