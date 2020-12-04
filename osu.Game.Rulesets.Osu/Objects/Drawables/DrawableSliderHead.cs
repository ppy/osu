// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSliderHead : DrawableHitCircle
    {
        [CanBeNull]
        public Slider Slider => DrawableSlider?.HitObject;

        protected DrawableSlider DrawableSlider => (DrawableSlider)ParentHitObject;

        private readonly IBindable<int> pathVersion = new Bindable<int>();

        protected override OsuSkinComponents CirclePieceComponent => OsuSkinComponents.SliderHeadHitCircle;

        public DrawableSliderHead()
        {
        }

        public DrawableSliderHead(SliderHeadCircle h)
            : base(h)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            PositionBindable.BindValueChanged(_ => updatePosition());
            pathVersion.BindValueChanged(_ => updatePosition());
        }

        protected override void OnFree()
        {
            base.OnFree();

            pathVersion.UnbindFrom(DrawableSlider.PathVersion);
        }

        protected override void OnApply()
        {
            base.OnApply();

            pathVersion.BindTo(DrawableSlider.PathVersion);

            OnShake = DrawableSlider.Shake;
            CheckHittable = (d, t) => DrawableSlider.CheckHittable?.Invoke(d, t) ?? true;
        }

        protected override void Update()
        {
            base.Update();

            Debug.Assert(Slider != null);

            double completionProgress = Math.Clamp((Time.Current - Slider.StartTime) / Slider.Duration, 0, 1);

            //todo: we probably want to reconsider this before adding scoring, but it looks and feels nice.
            if (!IsHit)
                Position = Slider.CurvePositionAt(completionProgress);
        }

        public Action<double> OnShake;

        public override void Shake(double maximumLength) => OnShake?.Invoke(maximumLength);

        private void updatePosition()
        {
            if (Slider != null)
                Position = HitObject.Position - Slider.Position;
        }
    }
}
