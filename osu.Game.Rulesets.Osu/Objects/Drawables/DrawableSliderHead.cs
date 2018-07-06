// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Rulesets.Objects.Types;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSliderHead : DrawableHitCircle
    {
        private readonly Slider slider;

        public DrawableSliderHead(Slider slider, HitCircle h)
            : base(h)
        {
            this.slider = slider;

            Position = HitObject.Position - slider.Position;
        }

        protected override void Update()
        {
            base.Update();

            double completionProgress = MathHelper.Clamp((Time.Current - slider.StartTime) / slider.Duration, 0, 1);

            //todo: we probably want to reconsider this before adding scoring, but it looks and feels nice.
            if (!IsHit)
                Position = slider.CurvePositionAt(completionProgress);
        }

        public Action<double> OnShake;

        protected override void Shake(double maximumLength) => OnShake?.Invoke(maximumLength);
    }
}
