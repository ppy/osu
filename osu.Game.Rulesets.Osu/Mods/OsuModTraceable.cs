// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Skinning.Default;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModTraceable : ModWithVisibilityAdjustment, IRequiresApproachCircles
    {
        public override string Name => "Traceable";
        public override string Acronym => "TC";
        public override ModType Type => ModType.Fun;
        public override LocalisableString Description => "Put your faith in the approach circles...";
        public override double ScoreMultiplier => 1;

        public override Type[] IncompatibleMods => new[] { typeof(IHidesApproachCircles) };

        /// <summary>
        /// Fade multipliers match hidden Mod for user convenience.
        /// </summary>
        private const double fade_in_duration_multiplier = 0.4;

        private const double fade_out_duration_multiplier = 0.3;

        [SettingSource("Fade out effect", "Hidden for approach circles!")]
        public Bindable<bool> FadeOutEffect { get; } = new BindableBool();

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            applyFadeOutState(hitObject);
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            applyTraceableState(hitObject, state);
            applyFadeOutState(hitObject);
        }

        private void applyTraceableState(DrawableHitObject drawable, ArmedState state)
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

        private void applyCirclePieceState(DrawableOsuHitObject hitObject, IDrawable? hitCircle = null)
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

        #region "Fade out effect" mod setting code.

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (!FadeOutEffect.Value) return;

            foreach (var obj in beatmap.HitObjects.OfType<OsuHitObject>())
                applyFadeInAdjustment(obj);

            static void applyFadeInAdjustment(OsuHitObject osuObject)
            {
                osuObject.TimeFadeIn = osuObject.TimePreempt * fade_in_duration_multiplier;
                foreach (var nested in osuObject.NestedHitObjects.OfType<OsuHitObject>())
                    applyFadeInAdjustment(nested);
            }
        }

        private void applyFadeOutState(DrawableHitObject drawableObject)
        {
            if (!FadeOutEffect.Value || drawableObject is not DrawableOsuHitObject drawableOsuObject) return;

            (double fadeStartTime, double fadeDuration) = getFadeOutParameters(drawableOsuObject);

            switch (drawableObject)
            {
                case DrawableSliderTail:
                    using (drawableObject.BeginAbsoluteSequence(fadeStartTime))
                        drawableObject.FadeOut(fadeDuration);

                    break;

                case DrawableSliderRepeat sliderRepeat:
                    using (drawableObject.BeginAbsoluteSequence(fadeStartTime))
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

        #endregion
    }
}
