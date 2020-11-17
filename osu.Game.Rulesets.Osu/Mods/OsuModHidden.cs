// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
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

        protected override bool IsFirstAdjustableObject(HitObject hitObject) => !(hitObject is Spinner);

        public override void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var d in drawables)
                d.HitObjectApplied += applyFadeInAdjustment;

            base.ApplyToDrawableHitObjects(drawables);
        }

        private void applyFadeInAdjustment(DrawableHitObject hitObject)
        {
            if (!(hitObject is DrawableOsuHitObject d))
                return;

            d.HitObject.TimeFadeIn = d.HitObject.TimePreempt * fade_in_duration_multiplier;

            foreach (var nested in d.NestedHitObjects)
                applyFadeInAdjustment(nested);
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

        private void applyState(DrawableHitObject drawable, bool increaseVisibility)
        {
            if (!(drawable is DrawableOsuHitObject d))
                return;

            var h = d.HitObject;

            var fadeOutStartTime = h.StartTime - h.TimePreempt + h.TimeFadeIn;
            var fadeOutDuration = h.TimePreempt * fade_out_duration_multiplier;

            // new duration from completed fade in to end (before fading out)
            var longFadeDuration = h.GetEndTime() - fadeOutStartTime;

            switch (drawable)
            {
                case DrawableSliderTail sliderTail:
                    // use stored values from head circle to achieve same fade sequence.
                    var tailFadeOutParameters = getFadeOutParametersFromSliderHead(h);

                    using (drawable.BeginAbsoluteSequence(tailFadeOutParameters.startTime, true))
                        sliderTail.FadeOut(tailFadeOutParameters.duration);

                    break;

                case DrawableSliderRepeat sliderRepeat:
                    // use stored values from head circle to achieve same fade sequence.
                    var repeatFadeOutParameters = getFadeOutParametersFromSliderHead(h);

                    using (drawable.BeginAbsoluteSequence(repeatFadeOutParameters.startTime, true))
                        // only apply to circle piece – reverse arrow is not affected by hidden.
                        sliderRepeat.CirclePiece.FadeOut(repeatFadeOutParameters.duration);

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
                        using (circle.BeginAbsoluteSequence(h.StartTime - h.TimePreempt, true))
                            circle.ApproachCircle.Hide();
                    }

                    // fade out immediately after fade in.
                    using (drawable.BeginAbsoluteSequence(fadeOutStartTime, true))
                        fadeTarget.FadeOut(fadeOutDuration);
                    break;

                case DrawableSlider slider:
                    associateNestedSliderCirclesWithHead(slider.HitObject);

                    using (slider.BeginAbsoluteSequence(fadeOutStartTime, true))
                        slider.Body.FadeOut(longFadeDuration, Easing.Out);

                    break;

                case DrawableSliderTick sliderTick:
                    // slider ticks fade out over up to one second
                    var tickFadeOutDuration = Math.Min(sliderTick.HitObject.TimePreempt - DrawableSliderTick.ANIM_DURATION, 1000);

                    using (sliderTick.BeginAbsoluteSequence(sliderTick.HitObject.StartTime - tickFadeOutDuration, true))
                        sliderTick.FadeOut(tickFadeOutDuration);

                    break;

                case DrawableSpinner spinner:
                    // hide elements we don't care about.
                    // todo: hide background

                    using (spinner.BeginAbsoluteSequence(fadeOutStartTime + longFadeDuration, true))
                        spinner.FadeOut(fadeOutDuration);

                    break;
            }
        }

        private readonly Dictionary<HitObject, SliderHeadCircle> correspondingSliderHeadForObject = new Dictionary<HitObject, SliderHeadCircle>();

        private void associateNestedSliderCirclesWithHead(Slider slider)
        {
            var sliderHead = slider.NestedHitObjects.Single(obj => obj is SliderHeadCircle);

            foreach (var nested in slider.NestedHitObjects)
            {
                if ((nested is SliderRepeat || nested is SliderEndCircle) && !correspondingSliderHeadForObject.ContainsKey(nested))
                    correspondingSliderHeadForObject[nested] = (SliderHeadCircle)sliderHead;
            }
        }

        private (double startTime, double duration) getFadeOutParametersFromSliderHead(OsuHitObject h)
        {
            var sliderHead = correspondingSliderHeadForObject[h];
            return (sliderHead.StartTime - sliderHead.TimePreempt + sliderHead.TimeFadeIn, sliderHead.TimePreempt * fade_out_duration_multiplier);
        }
    }
}
