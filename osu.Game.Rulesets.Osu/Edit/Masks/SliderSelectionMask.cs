// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Edit.Masks
{
    public class SliderSelectionMask : SelectionMask
    {
        private readonly SliderCircleSelectionMask headMask;

        public SliderSelectionMask(DrawableSlider slider)
            : base(slider)
        {
            var sliderObject = (Slider)slider.HitObject;

            InternalChildren = new Drawable[]
            {
                new SliderBodyMask(sliderObject),
                headMask = new SliderCircleSelectionMask(slider.HeadCircle, sliderObject, SliderPosition.Start),
                new SliderCircleSelectionMask(slider.TailCircle, sliderObject, SliderPosition.End),
            };
        }

        public override Vector2 SelectionPoint => headMask.SelectionPoint;
    }
}
