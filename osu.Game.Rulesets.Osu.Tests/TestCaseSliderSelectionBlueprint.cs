// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
    public class TestCaseSliderSelectionBlueprint : SelectionBlueprintTestCase
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

        public TestCaseSliderSelectionBlueprint()
        {
            var slider = new Slider
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
        }

        protected override SelectionBlueprint CreateBlueprint() => new SliderSelectionBlueprint(drawableObject);
    }
}
