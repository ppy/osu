// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.Osu.Skinning.Default;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModTraceable : ModWithVisibilityAdjustment, IRequiresApproachCircles
    {
        public override string Name => "Traceable";
        public override string Acronym => "TC";
        public override ModType Type => ModType.Fun;
        public override string Description => "Put your faith in the approach circles...";
        public override double ScoreMultiplier => 1;

        public override Type[] IncompatibleMods => new[] { typeof(IHidesApproachCircles) };

        /// <summary>
        /// Fade multipliers from Hidden Mod ported over to match player expectations of fade.
        /// </summary>
        private const double fade_in_duration_multiplier = 0.4;

        private const double fade_out_duration_multiplier = 0.3;

        [SettingSource("Fade out effect", "Hidden for approach circles!")]
        public Bindable<bool> FadeOutEffect { get; } = new BindableBool();

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            applyFadeOutState(hitObject, true);
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            applyTraceableState(hitObject);
            applyFadeOutState(hitObject, true);
        }

        private void applyTraceableState(DrawableHitObject drawable)
        {
            if (drawable is not DrawableOsuHitObject)
                return;

            //todo: expose and hide spinner background somehow

            switch (drawable)
            {
                case DrawableHitCircle circle:
                    // we only want to see the approach circle
                    applyCirclePieceState(circle, circle.CirclePiece);
                    break;

                case DrawableSliderTail sliderTail:
                    applyCirclePieceState(sliderTail);
                    break;

                case DrawableSliderRepeat sliderRepeat:
                    // show only the repeat arrow
                    applyCirclePieceState(sliderRepeat, sliderRepeat.CirclePiece);
                    break;

                case DrawableSlider slider:
                    slider.Body.OnSkinChanged += () => applySliderState(slider);
                    applySliderState(slider);
                    break;
            }
        }

        private void applyCirclePieceState(DrawableOsuHitObject hitObject, IDrawable hitCircle = null)
        {
            var h = hitObject.HitObject;
            using (hitObject.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                (hitCircle ?? hitObject).Hide();
        }

        private void applySliderState(DrawableSlider slider)
        {
            ((PlaySliderBody)slider.Body.Drawable).AccentColour = slider.AccentColour.Value.Opacity(0);
            ((PlaySliderBody)slider.Body.Drawable).BorderColour = slider.AccentColour.Value;
        }

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            foreach (var obj in beatmap.HitObjects.OfType<OsuHitObject>())
                applyFadeInAdjustment(obj);

            static void applyFadeInAdjustment(OsuHitObject osuObject)
            {
                osuObject.TimeFadeIn = osuObject.TimePreempt * fade_in_duration_multiplier;
                foreach (var nested in osuObject.NestedHitObjects.OfType<OsuHitObject>())
                    applyFadeInAdjustment(nested);
            }
        }

        /// <summary>
        /// Code for Applying fade out for the "Fade out effect" mod setting.
        /// </summary>
        private void applyFadeOutState(DrawableHitObject drawableObject, bool increaseVisibility)
        {
            if (!FadeOutEffect.Value) return;
            if (drawableObject is not DrawableOsuHitObject drawableOsuObject)
                return;

            (double fadeStartTime, double fadeDuration) = getFadeOutParameters(drawableOsuObject);

            if (increaseVisibility)
            {
                if (drawableObject is DrawableSpinner spinner)
                {
                    spinner.Body.OnSkinChanged += () => hideSpinnerApproachCircle(spinner);
                    hideSpinnerApproachCircle(spinner);
                }
            }

            switch (drawableObject)
            {
                case DrawableSliderTail:
                    using (drawableObject.BeginAbsoluteSequence(fadeStartTime))
                        drawableObject.FadeOut(fadeDuration);

                    break;

                case DrawableSliderRepeat sliderRepeat:
                    using (drawableObject.BeginAbsoluteSequence(fadeStartTime))
                        // only apply to circle piece – reverse arrow is not affected by hidden.
                        sliderRepeat.CirclePiece.FadeOut(fadeDuration);

                    break;

                case DrawableHitCircle circle:
                    Drawable fadeTarget = circle.ApproachCircle;
                    using (drawableObject.BeginAbsoluteSequence(fadeStartTime))
                        fadeTarget.FadeOut(fadeDuration);
                    break;

                case DrawableSlider slider:
                    using (slider.BeginAbsoluteSequence(fadeStartTime))
                        slider.Body.FadeOut(fadeDuration, Easing.Out);

                    break;

                case DrawableSliderTick sliderTick:
                    using (sliderTick.BeginAbsoluteSequence(fadeStartTime))
                        sliderTick.FadeOut(fadeDuration);

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
                double fadeOutDuration = hitObject.TimePreempt * fade_out_duration_multiplier;

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

        private void hideSpinnerApproachCircle(DrawableSpinner spinner)
        {
            if (!FadeOutEffect.Value) return;

            var spinnerBody = ((IHasApproachCircle)spinner.Body.Drawable)?.ApproachCircle;
            if (spinnerBody == null)
                return;

            using (spinner.BeginAbsoluteSequence(spinner.HitObject.StartTime - spinner.HitObject.TimePreempt))
                spinnerBody.Hide();
        }
    }
}
