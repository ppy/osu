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

        private double fadeOutStartTime { get; set; }
        private double fadeOutDuration { get; set; }
        private double longFadeDuration { get; set; }

        public override void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            static void adjustFadeIn(OsuHitObject h) => h.TimeFadeIn = h.TimePreempt * fade_in_duration_multiplier;

            foreach (var d in drawables.OfType<DrawableOsuHitObject>())
            {
                adjustFadeIn(d.HitObject);
                foreach (var h in d.HitObject.NestedHitObjects.OfType<OsuHitObject>())
                    adjustFadeIn(h);
            }

            if (IncreaseFirstObjectVisibility.Value)
            {
                int spinnerCount = checkForSpinners(drawables);

                if (spinnerCount > 0)
                {
                    IEnumerable<DrawableHitObject> startingSpinners = drawables.Take(spinnerCount);
                    drawables = drawables.Skip(spinnerCount);

                    foreach (var d in startingSpinners)
                        d.ApplyCustomUpdateState += ApplyHiddenState;
                }

                DrawableHitObject firstHitObject = drawables.First();
                firstHitObject.ApplyCustomUpdateState += ApplySpecialHiddenState;
            }

            base.ApplyToDrawableHitObjects(drawables);
        }

        protected override void ApplyHiddenState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableOsuHitObject d))
                return;

            var h = d.HitObject;
            setFadeTimes(h);

            switch (drawable)
            {
                case DrawableHitCircle circle:
                    // we don't want to see the approach circle
                    using (circle.BeginAbsoluteSequence(h.StartTime - h.TimePreempt, true))
                        circle.ApproachCircle.Hide();

                    // fade out immediately after fade in.
                    using (drawable.BeginAbsoluteSequence(fadeOutStartTime, true))
                        circle.FadeOut(fadeOutDuration);

                    break;

                case DrawableSlider slider:
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
                    spinner.Disc.Hide();
                    spinner.Ticks.Hide();
                    spinner.Background.Hide();

                    using (spinner.BeginAbsoluteSequence(fadeOutStartTime + longFadeDuration, true))
                        spinner.FadeOut(fadeOutDuration);

                    break;
            }
        }

        /// <summary>
        /// A hidden state that keeps the approach circle visible by only fading out circlepiece.
        /// true This state is applied to the first circle or slider
        /// when <see cref="ModHidden.IncreaseFirstObjectVisibility"/> is set to true.
        /// </summary>
        protected void ApplySpecialHiddenState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableOsuHitObject d))
                return;

            var h = d.HitObject;
            setFadeTimes(h);

            switch (drawable)
            {
                case DrawableHitCircle circle:
                    using (drawable.BeginAbsoluteSequence(fadeOutStartTime, true))
                        circle.CirclePiece.FadeOut(fadeOutDuration);

                    break;

                case DrawableSlider slider:
                    using (slider.BeginAbsoluteSequence(fadeOutStartTime, true))
                        slider.Body.FadeOut(longFadeDuration, Easing.Out);

                    break;
            }
        }

        private int checkForSpinners(IEnumerable<DrawableHitObject> drawables)
        {
            int startingSpinnersCount = 0;

            foreach (var d in drawables.OfType<DrawableOsuHitObject>())
            {
                if (d is DrawableSpinner)
                    ++startingSpinnersCount;
                else
                    return startingSpinnersCount;
            }

            return startingSpinnersCount;
        }

        private void setFadeTimes(OsuHitObject h)
        {
            fadeOutStartTime = h.StartTime - h.TimePreempt + h.TimeFadeIn;
            fadeOutDuration = h.TimePreempt * fade_out_duration_multiplier;

            // new duration from completed fade in to end (before fading out)
            longFadeDuration = h.GetEndTime() - fadeOutStartTime;
        }
    }
}
