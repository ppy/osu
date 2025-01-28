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

        public override bool DisplayResult
        {
            get
            {
                if (HitObject?.ClassicSliderBehaviour == true)
                    return false;

                return base.DisplayResult;
            }
        }

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

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            base.CheckForResult(userTriggered, timeOffset);
            DrawableSlider.SliderInputManager.PostProcessHeadJudgement(this);
        }

        protected override HitResult ResultFor(double timeOffset)
        {
            Debug.Assert(HitObject != null);

            if (HitObject.ClassicSliderBehaviour)
            {
                // With classic slider behaviour, heads are considered fully hit if in the largest hit window.
                // We can't award a full Great because the true Great judgement is awarded on the Slider itself,
                // reduced based on number of ticks hit,
                // so we use the most suitable LargeTick judgement here instead.
                return base.ResultFor(timeOffset).IsHit() ? HitResult.LargeTickHit : HitResult.LargeTickMiss;
            }

            return base.ResultFor(timeOffset);
        }

        public override void Shake()
        {
            base.Shake();
            DrawableSlider.Shake();
        }
    }
}
