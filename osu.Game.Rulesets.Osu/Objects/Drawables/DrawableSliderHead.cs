// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSliderHead : DrawableHitCircle
    {
        public DrawableSliderHead(Slider slider, HitCircle h)
            : base(h)
        {
            Position = HitObject.Position - slider.Position;
        }
    }
}
