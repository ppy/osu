// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModHidden : ModHidden, IHidesApproachCircles, IApplicableToHealthProcessor, IUpdatableByPlayfield
    {
        [SettingSource("Only fade approach circles", "The main object body will not fade when enabled.")]
        public Bindable<bool> OnlyFadeApproachCircles { get; } = new BindableBool();

        public override LocalisableString Description => @"Play with no approach circles and fading circles/sliders.";
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.06 : 1;

        public override Type[] IncompatibleMods => new[] { typeof(IRequiresApproachCircles), typeof(OsuModSpinIn), typeof(OsuModDepth) };

        public const double FADE_IN_DURATION_MULTIPLIER = 0.4;
        public const double FADE_OUT_DURATION_MULTIPLIER = 0.3;

        protected override bool IsFirstAdjustableObject(HitObject hitObject) => !(hitObject is Spinner || hitObject is SpinnerTick);

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            base.ApplyToBeatmap(beatmap);

            foreach (var obj in beatmap.HitObjects.OfType<OsuHitObject>())
                applyFadeInAdjustment(obj);

            static void applyFadeInAdjustment(OsuHitObject osuObject)
            {
                osuObject.TimeFadeIn = osuObject.TimePreempt * FADE_IN_DURATION_MULTIPLIER;
                foreach (var nested in osuObject.NestedHitObjects.OfType<OsuHitObject>())
                    applyFadeInAdjustment(nested);
            }
        }

        public void Update(Playfield playfield)
        {
            foreach (var drawableHitObject in playfield.HitObjectContainer.AliveObjects)
            {
                var target = (drawableHitObject switch
                {
                    DrawableHitCircle drawableHitCircle => drawableHitCircle,
                    DrawableSlider drawableSlider => drawableSlider.HeadCircle,
                    _ => null
                });
                if (target == null)
                    continue;

                var hitObject = target.HitObject;
                double fadeOutStartTime = hitObject.StartTime - hitObject.TimePreempt + hitObject.TimeFadeIn;
                double fadeOutDuration = hitObject.TimePreempt * fadeDurationFactor;

                using (target.BeginAbsoluteSequence(fadeOutStartTime))
                {
                    target.ApproachCircle.FadeOutFromOne(fadeOutDuration);
                }
            }
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            applyHiddenState(hitObject, true);
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            applyHiddenState(hitObject, false);
        }

        private void applyHiddenState(DrawableHitObject drawableObject, bool increaseVisibility)
        {
            if (!(drawableObject is DrawableOsuHitObject drawableOsuObject))
                return;

            OsuHitObject hitObject = drawableOsuObject.HitObject;

            (double fadeStartTime, double fadeDuration) = getFadeOutParameters(drawableOsuObject);

            // process approach circle hiding first (to allow for early return below).
            if (!increaseVisibility)
            {
                if (drawableObject is DrawableHitCircle circle)
                {
                    using (circle.BeginAbsoluteSequence(hitObject.StartTime - hitObject.TimePreempt))

                        circle.ApproachCircle.FadeTo(this.fadeDurationFactor, fadeDuration, Easing.InQuart);
                }
                else if (drawableObject is DrawableSpinner spinner)
                {
                    spinner.Body.OnSkinChanged += () => hideSpinnerApproachCircle(spinner);
                    hideSpinnerApproachCircle(spinner);
                }
            }

            if (OnlyFadeApproachCircles.Value)
                return;

            switch (drawableObject)
            {
                case DrawableSliderTail:
                    using (drawableObject.BeginAbsoluteSequence(fadeStartTime))
                        drawableObject.FadeOut(fadeDuration);

                    break;

                case DrawableSliderRepeat sliderRepeat:
                    using (drawableObject.BeginAbsoluteSequence(fadeStartTime))
                        // only apply to circle piece – reverse arrow is not affected by hidden.
                        //sliderRepeat.CirclePiece.Hide();
                        sliderRepeat.CirclePiece.FadeOut(fadeDuration);

                    using (drawableObject.BeginAbsoluteSequence(drawableObject.HitStateUpdateTime))
                        sliderRepeat.FadeOut();

                    break;

                case DrawableHitCircle circle:
                    Drawable fadeTarget = circle;

                    if (increaseVisibility || true)
                    {
                        // only fade the circle piece (not the approach circle) for the increased visibility object.
                        fadeTarget = circle.CirclePiece;
                    }

                    using (drawableObject.BeginAbsoluteSequence(fadeStartTime))
                        fadeTarget.FadeTo(alpha, fadeDuration);
                    break;

                case DrawableSlider slider:
                    using (slider.BeginAbsoluteSequence(fadeStartTime))
                        slider.Body.FadeTo(alpha, fadeDuration, Easing.Out);

                    break;

                case DrawableSliderTick sliderTick:
                    using (sliderTick.BeginAbsoluteSequence(fadeStartTime))
                        sliderTick.FadeTo(alpha, fadeDuration);

                    break;

                case DrawableSpinner spinner:
                    // hide elements we don't care about.
                    // todo: hide background

                    using (spinner.BeginAbsoluteSequence(fadeStartTime))
                        spinner.FadeOut(fadeDuration);

                    break;
            }
        }

        private (double fadeStartTime, double fadeDuration) getFadeOutParameters(DrawableOsuHitObject drawableObject)
        {
            switch (drawableObject)
            {
                case DrawableSliderTail tail:
                    // Use the same fade sequence as the slider head.
                    Debug.Assert(tail.Slider != null);
                    return getParameters(tail.Slider.HeadCircle);

                case DrawableSliderRepeat repeat:
                    // Use the same fade sequence as the slider head.
                    Debug.Assert(repeat.Slider != null);
                    return getParameters(repeat.Slider.HeadCircle);

                default:
                    return getParameters(drawableObject.HitObject);
            }

            static (double fadeStartTime, double fadeDuration) getParameters(OsuHitObject hitObject)
            {
                double fadeOutStartTime = hitObject.StartTime - hitObject.TimePreempt + hitObject.TimeFadeIn;
                double fadeOutDuration = hitObject.TimePreempt * FADE_OUT_DURATION_MULTIPLIER;

                // new duration from completed fade in to end (before fading out)
                double longFadeDuration = hitObject.GetEndTime() - fadeOutStartTime;

                switch (hitObject)
                {
                    case Slider:
                        return (fadeOutStartTime, longFadeDuration);

                    case SliderTick:
                        double tickFadeOutDuration = Math.Min(hitObject.TimePreempt - DrawableSliderTick.ANIM_DURATION, 1000);
                        return (hitObject.StartTime - tickFadeOutDuration, tickFadeOutDuration);

                    case Spinner:
                        return (fadeOutStartTime + longFadeDuration, fadeOutDuration);

                    default:
                        return (fadeOutStartTime, fadeOutDuration);
                }
            }
        }

        private static void hideSpinnerApproachCircle(DrawableSpinner spinner)
        {
            var approachCircle = (spinner.Body.Drawable as IHasApproachCircle)?.ApproachCircle;
            if (approachCircle == null)
                return;

            using (spinner.BeginAbsoluteSequence(spinner.HitObject.StartTime - spinner.HitObject.TimePreempt))
                approachCircle.Hide();
        }

        private float alpha = 0;
        private float fadeDurationFactor = 0.9f;

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            healthProcessor.Health.ValueChanged += newHealth =>
            {
                fadeDurationFactor = Math.Clamp(0.75f - (float)newHealth.NewValue, 0.1f, 0.9f);
            };
        }
    }
}
