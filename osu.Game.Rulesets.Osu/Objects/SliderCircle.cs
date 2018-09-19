// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SliderCircle : HitCircle
    {
        private readonly Slider slider;

        public SliderCircle(Slider slider)
        {
            this.slider = slider;
        }

        public override void AdjustPosition(Vector2 position) => slider.AdjustPosition(position);
    }
}
