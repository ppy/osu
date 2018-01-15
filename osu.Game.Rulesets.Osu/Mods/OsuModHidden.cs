// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModHidden : ModHidden, IApplicableToDrawableHitObjects
    {
        public override string Description => @"Play with no approach circles and fading notes for a slight score advantage.";
        public override double ScoreMultiplier => 1.06;

        private const float fade_in_duration_multiplier = 0.4f;
        private const double fade_out_duration_multiplier = 0.3;

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var d in drawables.OfType<DrawableOsuHitObject>())
            {
                d.ApplyCustomUpdateState += ApplyHiddenState;
                d.HitObject.TimeFadein = d.HitObject.TimePreempt * fade_in_duration_multiplier;
            }
        }

        protected void ApplyHiddenState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableOsuHitObject d))
                return;

            var fadeOutStartTime = d.HitObject.StartTime - d.HitObject.TimePreempt + d.HitObject.TimeFadein;
            var fadeOutDuration = d.HitObject.TimePreempt * fade_out_duration_multiplier;

            // new duration from completed fade in to end (before fading out)
            var longFadeDuration = ((d.HitObject as IHasEndTime)?.EndTime ?? d.HitObject.StartTime) - fadeOutStartTime;

            switch (drawable)
            {
                case DrawableHitCircle circle:
                    // we don't want to see the approach circle
                    circle.ApproachCircle.Hide();

                    // fade out immediately after fade in.
                    using (drawable.BeginAbsoluteSequence(fadeOutStartTime, true))
                    {
                        circle.FadeOut(fadeOutDuration);
                    }

                    break;
                case DrawableSlider slider:
                    using (slider.BeginAbsoluteSequence(fadeOutStartTime, true))
                    {
                        slider.Body.FadeOut(longFadeDuration, Easing.Out);
                    }

                    break;
                case DrawableSpinner spinner:
                    // hide elements we don't care about.
                    spinner.Disc.Hide();
                    spinner.Ticks.Hide();
                    spinner.Background.Hide();

                    using (spinner.BeginAbsoluteSequence(fadeOutStartTime + longFadeDuration, true))
                    {
                        spinner.FadeOut(fadeOutDuration);
                    }

                    break;
            }
        }
    }
}
