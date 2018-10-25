// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Masks.Slider;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestCaseSliderSelectionMask : HitObjectSelectionMaskTestCase
    {
        private readonly DrawableSlider drawableObject;

        public TestCaseSliderSelectionMask()
        {
            var slider = new Slider
            {
                Position = new Vector2(256, 192),
                ControlPoints = new List<Vector2>
                {
                    Vector2.Zero,
                    new Vector2(150, 150),
                    new Vector2(300, 0)
                },
                CurveType = CurveType.Bezier,
                Distance = 350
            };

            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 2 });

            Add(drawableObject = new DrawableSlider(slider));
        }

        protected override SelectionMask CreateMask() => new SliderSelectionMask(drawableObject);
    }
}
