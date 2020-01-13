// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSliderHead : DrawableHitCircle
    {
        private readonly IBindable<Vector2> positionBindable = new Bindable<Vector2>();
        private readonly IBindable<int> pathVersion = new Bindable<int>();

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
            pathVersion.BindTo(slider.Path.Version);

            positionBindable.BindValueChanged(_ => updatePosition());
            pathVersion.BindValueChanged(_ => updatePosition(), true);
        }

        public Action<double> OnShake;

        protected override void Shake(double maximumLength) => OnShake?.Invoke(maximumLength);

        private void updatePosition() => Position = HitObject.Position - slider.Position;
    }
}
