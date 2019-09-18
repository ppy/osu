// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSliderHead : DrawableHitCircle
    {
        private readonly IBindable<Vector2> positionBindable = new Bindable<Vector2>();
        private readonly IBindable<SliderPath> pathBindable = new Bindable<SliderPath>();

        private readonly Slider slider;

        public DrawableSliderHead(Slider slider, HitCircle h)
            : base(h)
        {
            this.slider = slider;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            positionBindable.BindTo(HitObject.PositionBindable);
            pathBindable.BindTo(slider.PathBindable);

            positionBindable.BindValueChanged(_ => updatePosition());
            pathBindable.BindValueChanged(_ => updatePosition(), true);
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

        private void updatePosition() => Position = HitObject.Position - slider.Position;
    }
}
