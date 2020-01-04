// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableRepeatPoint : DrawableOsuHitObject, ITrackSnaking
    {
        private readonly RepeatPoint repeatPoint;
        private readonly DrawableSlider drawableSlider;

        public readonly Bindable<ReverseArrowPulseMode> PulseMode = new Bindable<ReverseArrowPulseMode>(ReverseArrowPulseMode.Synced);

        private double animDuration;

        private readonly Drawable scaleContainer;

        [Resolved(CanBeNull = true)]
        private OsuRulesetConfigManager config { get; set; }

        public DrawableRepeatPoint(RepeatPoint repeatPoint, DrawableSlider drawableSlider)
            : base(repeatPoint)
        {
            this.repeatPoint = repeatPoint;
            this.drawableSlider = drawableSlider;

            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);

            Blending = BlendingParameters.Additive;
            Origin = Anchor.Centre;

            InternalChild = scaleContainer = new ReverseArrowPiece();
        }

        private readonly IBindable<float> scaleBindable = new Bindable<float>();

        [BackgroundDependencyLoader]
        private void load()
        {
            scaleBindable.BindValueChanged(scale => scaleContainer.Scale = new Vector2(scale.NewValue), true);
            scaleBindable.BindTo(HitObject.ScaleBindable);

            config?.BindWith(OsuRulesetSetting.ReverseArrowPulse, PulseMode);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (repeatPoint.StartTime <= Time.Current)
                ApplyResult(r => r.Type = drawableSlider.Tracking.Value ? HitResult.Great : HitResult.Miss);
        }

        protected override void UpdateInitialTransforms()
        {
            animDuration = Math.Min(300, repeatPoint.SpanDuration);

            this.FadeIn(animDuration);

            if (PulseMode.Value == ReverseArrowPulseMode.Stable)
            {
                double fadeInStart = repeatPoint.StartTime - 2 * repeatPoint.SpanDuration;

                // We want first repeat arrow to start pulsing during snake in
                if (repeatPoint.RepeatIndex == 0)
                    fadeInStart -= repeatPoint.TimePreempt;

                for (double pulseStartTime = fadeInStart; pulseStartTime < repeatPoint.StartTime; pulseStartTime += 300)
                    this.Delay(pulseStartTime - LifetimeStart).ScaleTo(1.3f).ScaleTo(1f, Math.Min(300, repeatPoint.StartTime - pulseStartTime), Easing.Out);
            }
            else if (PulseMode.Value == ReverseArrowPulseMode.Off)
                this.ScaleTo(0.5f).ScaleTo(1f, animDuration * 2, Easing.OutElasticHalf);
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            switch (state)
            {
                case ArmedState.Idle:
                    this.Delay(HitObject.TimePreempt).FadeOut();
                    break;

                case ArmedState.Miss:
                    this.FadeOut(animDuration);
                    break;

                case ArmedState.Hit:
                    this.FadeOut(animDuration, Easing.Out)
                        .ScaleTo(Scale * 1.5f, animDuration, Easing.Out);
                    break;
            }
        }

        private bool hasRotation;

        public void UpdateSnakingPosition(Vector2 start, Vector2 end)
        {
            bool isRepeatAtEnd = repeatPoint.RepeatIndex % 2 == 0;
            List<Vector2> curve = ((PlaySliderBody)drawableSlider.Body.Drawable).CurrentCurve;

            Position = isRepeatAtEnd ? end : start;

            if (curve.Count < 2)
                return;

            int searchStart = isRepeatAtEnd ? curve.Count - 1 : 0;
            int direction = isRepeatAtEnd ? -1 : 1;

            Vector2 aimRotationVector = Vector2.Zero;

            // find the next vector2 in the curve which is not equal to our current position to infer a rotation.
            for (int i = searchStart; i >= 0 && i < curve.Count; i += direction)
            {
                if (Precision.AlmostEquals(curve[i], Position))
                    continue;

                aimRotationVector = curve[i];
                break;
            }

            float aimRotation = MathHelper.RadiansToDegrees(MathF.Atan2(aimRotationVector.Y - Position.Y, aimRotationVector.X - Position.X));
            while (Math.Abs(aimRotation - Rotation) > 180)
                aimRotation += aimRotation < Rotation ? 360 : -360;

            if (!hasRotation)
            {
                Rotation = aimRotation;
                hasRotation = true;
            }
            else
            {
                // If we're already snaking, interpolate to smooth out sharp curves (linear sliders, mainly).
                Rotation = Interpolation.ValueAt(Math.Clamp(Clock.ElapsedFrameTime, 0, 100), Rotation, aimRotation, 0, 50, Easing.OutQuint);
            }
        }
    }
}
