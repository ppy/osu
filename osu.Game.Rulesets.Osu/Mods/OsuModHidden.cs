// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModHidden : ModHidden
    {
        public override string Description => @"Play with no approach circles and fading circles/sliders.";
        public override double ScoreMultiplier => 1.06;

        public override Type[] IncompatibleMods => new[] { typeof(OsuModTraceable), typeof(OsuModSpinIn) };

        private const double fade_in_duration_multiplier = 0.4;
        private const double fade_out_duration_multiplier = 0.3;

        protected override bool IsFirstAdjustableObject(HitObject hitObject) => !(hitObject is Spinner || hitObject is SpinnerTick);

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            base.ApplyToBeatmap(beatmap);

            foreach (var obj in beatmap.HitObjects.OfType<OsuHitObject>())
                applyFadeInAdjustment(obj);

            static void applyFadeInAdjustment(OsuHitObject osuObject)
            {
                osuObject.TimeFadeIn = osuObject.TimePreempt * fade_in_duration_multiplier;
                foreach (var nested in osuObject.NestedHitObjects.OfType<OsuHitObject>())
                    applyFadeInAdjustment(nested);
            }
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            base.ApplyIncreasedVisibilityState(hitObject, state);
            applyState(hitObject, true);
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            base.ApplyNormalVisibilityState(hitObject, state);
            applyState(hitObject, false);
        }

        private void applyState(DrawableHitObject drawableObject, bool increaseVisibility)
        {
            if (!(drawableObject is DrawableOsuHitObject drawableOsuObject))
                return;

            OsuHitObject hitObject = drawableOsuObject.HitObject;

            (double startTime, double duration) fadeOut = getFadeOutParameters(drawableOsuObject);

            switch (drawableObject)
            {
                case DrawableSliderTail _:
                    using (drawableObject.BeginAbsoluteSequence(fadeOut.startTime, true))
                        drawableObject.FadeOut(fadeOut.duration);

                    break;

                case DrawableSliderRepeat sliderRepeat:
                    using (drawableObject.BeginAbsoluteSequence(fadeOut.startTime, true))
                        // only apply to circle piece – reverse arrow is not affected by hidden.
                        sliderRepeat.CirclePiece.FadeOut(fadeOut.duration);

                    break;

                case DrawableHitCircle circle:
                    Drawable fadeTarget = circle;

                    if (increaseVisibility)
                    {
                        // only fade the circle piece (not the approach circle) for the increased visibility object.
                        fadeTarget = circle.CirclePiece;
                    }
                    else
                    {
                        // we don't want to see the approach circle
                        using (circle.BeginAbsoluteSequence(hitObject.StartTime - hitObject.TimePreempt, true))
                            circle.ApproachCircle.Hide();
                    }

                    using (drawableObject.BeginAbsoluteSequence(fadeOut.startTime, true))
                        fadeTarget.FadeOut(fadeOut.duration);
                    break;

                case DrawableSlider slider:
                    using (slider.BeginAbsoluteSequence(fadeOut.startTime, true))
                        slider.Body.FadeOut(fadeOut.duration, Easing.Out);

                    break;

                case DrawableSliderTick sliderTick:
                    using (sliderTick.BeginAbsoluteSequence(fadeOut.startTime, true))
                        sliderTick.FadeOut(fadeOut.duration);

                    break;

                case DrawableSpinner spinner:
                    // hide elements we don't care about.
                    // todo: hide background

                    using (spinner.BeginAbsoluteSequence(fadeOut.startTime, true))
                        spinner.FadeOut(fadeOut.duration);

                    break;
            }
        }

        private (double startTime, double duration) getFadeOutParameters(DrawableOsuHitObject drawableObject)
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

            static (double startTime, double duration) getParameters(OsuHitObject hitObject)
            {
                var fadeOutStartTime = hitObject.StartTime - hitObject.TimePreempt + hitObject.TimeFadeIn;
                var fadeOutDuration = hitObject.TimePreempt * fade_out_duration_multiplier;

                // new duration from completed fade in to end (before fading out)
                var longFadeDuration = hitObject.GetEndTime() - fadeOutStartTime;

                switch (hitObject)
                {
                    case Slider _:
                        return (fadeOutStartTime, longFadeDuration);

                    case SliderTick _:
                        var tickFadeOutDuration = Math.Min(hitObject.TimePreempt - DrawableSliderTick.ANIM_DURATION, 1000);
                        return (hitObject.StartTime - tickFadeOutDuration, tickFadeOutDuration);

                    case Spinner _:
                        return (fadeOutStartTime + longFadeDuration, fadeOutDuration);

                    default:
                        return (fadeOutStartTime, fadeOutDuration);
                }
            }
        }
    }
}
