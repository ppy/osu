﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Diagnostics;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public partial class DrawableSliderHead : DrawableHitCircle
    {
        public new SliderHeadCircle HitObject => (SliderHeadCircle)base.HitObject;

        public DrawableSlider DrawableSlider => (DrawableSlider)ParentHitObject;

        public override bool DisplayResult => false;

        private readonly IBindable<int> pathVersion = new Bindable<int>();

        protected override OsuSkinComponents CirclePieceComponent => OsuSkinComponents.SliderHeadHitCircle;

        public DrawableSliderHead()
        {
        }

        public DrawableSliderHead(SliderHeadCircle h)
            : base(h)
        {
        }

        protected override void OnFree()
        {
            base.OnFree();

            pathVersion.UnbindFrom(DrawableSlider.PathVersion);
        }

        protected override void UpdatePosition()
        {
            // Slider head is always drawn at (0,0).
        }

        protected override void OnApply()
        {
            base.OnApply();

            pathVersion.BindTo(DrawableSlider.PathVersion);

            CheckHittable = (d, t, r) => DrawableSlider.CheckHittable?.Invoke(d, t, r) ?? ClickAction.Hit;
        }

        protected override HitResult ResultFor(double timeOffset)
        {
            Debug.Assert(HitObject != null);

            return base.ResultFor(timeOffset).IsHit()
                ? Result.Judgement.MaxResult
                : Result.Judgement.MinResult;
        }

        public override void Shake()
        {
            base.Shake();
            DrawableSlider.Shake();
        }
    }
}
