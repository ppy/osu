// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
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

        private readonly DrawableSlider drawableObject;

        private TestSliderSelectionBlueprint blueprint;

        private readonly Slider slider;

        private readonly Vector2 startPosition = new Vector2(256, 192);

        private readonly SliderPath startPath = new SliderPath(PathType.Bezier, new[]
        {
            Vector2.Zero,
            new Vector2(150, 150),
            new Vector2(300, 0)
        });

        public TestSceneSliderSelectionBlueprint()
        {
            slider = new Slider
            {
                Position = startPosition,
                Path = startPath
            };

            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 2 });

            Add(drawableObject = new DrawableSlider(slider));
        }

        [SetUp]
        public void SetUp()
        {
            Schedule(() =>
            {
                slider.Position = startPosition;
                slider.Path = startPath;
            });
        }

        [Test]
        public void TestModifyPointRegeneratesCircles()
        {
            bool regenerated = false;

            AddStep("Modify sliderpath", () =>
            {
                regenerated = false;
                slider.OnTicksRegenerated += () => regenerated = true;

                slider.Path = new SliderPath(PathType.Bezier, new[]
                {
                    Vector2.Zero,
                    new Vector2(-150, -150),
                    new Vector2(300, 0)
                });
            });

            AddAssert("Nested objects were regenerated", () => regenerated);

            AddStep("Move slider", () => slider.Position = new Vector2(192, 256));

            // We should have only regenerated the ticks and nothing else, so check that the selection blueprint is still attached to the head circle.
            AddAssert("Selection blueprint attatched to drawable", () => blueprint.HeadBlueprint.HitObject.HitObject == slider.HeadCircle);
            AddAssert("Slider head blueprint tracks object", () => blueprint.HeadBlueprint.Position == drawableObject.HeadCircle.Position);
        }

        protected override SelectionBlueprint CreateBlueprint() => blueprint = new TestSliderSelectionBlueprint(drawableObject);

        private class TestSliderSelectionBlueprint : SliderSelectionBlueprint
        {
            public new SliderCircleSelectionBlueprint HeadBlueprint => base.HeadBlueprint;

            public TestSliderSelectionBlueprint(DrawableSlider slider)
                : base(slider)
            {
            }
        }
    }
}
