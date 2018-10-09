// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModTraceable : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Traceable";
        public override string ShortenedName => "TC";
        public override FontAwesome Icon => FontAwesome.fa_snapchat_ghost;
        public override ModType Type => ModType.Fun;
        public override string Description => "Put your faith in the approach circles...";
        public override double ScoreMultiplier => 1;

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables)
                drawable.ApplyCustomUpdateState += ApplyTraceableState;
        }

        /* Similar to ApplyHiddenState, only different if drawable is DrawableHitCircle.
         * If we'd use ApplyHiddenState instead but only on non-DrawableHitCircle's, then 
         * the nested object HeadCircle of DrawableSlider would still use ApplyHiddenState,
         * thus treating the DrawableHitCircle with the hidden mod instead of the traceable mod.
         */
        protected void ApplyTraceableState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableOsuHitObject d))
                return;

            var h = d.HitObject;

            var fadeOutStartTime = h.StartTime - h.TimePreempt + h.TimeFadeIn;

            // new duration from completed fade in to end (before fading out)
            var longFadeDuration = ((h as IHasEndTime)?.EndTime ?? h.StartTime) - fadeOutStartTime;

            switch (drawable)
            {
                case DrawableHitCircle circle:
                    // we only want to see the approach circle
                    using (circle.BeginAbsoluteSequence(h.StartTime - h.TimePreempt, true))
                        circle.HideButApproachCircle();

                    // approach circle fades out quickly at StartTime
                    using (drawable.BeginAbsoluteSequence(h.StartTime, true))
                        circle.ApproachCircle.FadeOut(50);

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
                        spinner.FadeOut(h.TimePreempt);

                    break;
            }
        }
    }
}
