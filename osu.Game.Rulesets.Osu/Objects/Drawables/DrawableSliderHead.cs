// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSliderHead : DrawableHitCircle
    {
        private readonly IBindable<int> pathVersion = new Bindable<int>();

        protected override OsuSkinComponents CirclePieceComponent => OsuSkinComponents.SliderHeadHitCircle;

        private readonly Slider slider;

        public DrawableSliderHead(Slider slider, SliderHeadCircle h)
            : base(h)
        {
            this.slider = slider;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            pathVersion.BindTo(slider.Path.Version);

            PositionBindable.BindValueChanged(_ => updatePosition());
            pathVersion.BindValueChanged(_ => updatePosition(), true);
        }

        protected override void Update()
        {
            base.Update();

            double completionProgress = Math.Clamp((Time.Current - slider.StartTime) / slider.Duration, 0, 1);

            //todo: we probably want to reconsider this before adding scoring, but it looks and feels nice.
            if (!IsHit)
                Position = slider.CurvePositionAt(completionProgress);
        }

        public Action<double> OnShake;

        protected override void Shake(double maximumLength) => OnShake?.Invoke(maximumLength);

        private void updatePosition() => Position = HitObject.Position - slider.Position;
    }
}
