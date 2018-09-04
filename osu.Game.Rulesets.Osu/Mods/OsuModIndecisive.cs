
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Graphics;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModIndecisive : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Indecisive";
        public override string ShortenedName => "ID";
        public override FontAwesome Icon => FontAwesome.fa_question_circle;
        public override ModType Type => ModType.Fun;
        public override string Description => "Nomod-Hidden alternation.";
        public override double ScoreMultiplier => 1;

        private const double fade_in_duration_multiplier = 0.4;
        private const double fade_out_duration_multiplier = 0.3;

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            void adjustFadeIn(OsuHitObject h) => h.TimeFadeIn = h.TimePreempt * fade_in_duration_multiplier;

            foreach (var d in drawables.Where((x, i) => i % 2 == 0).OfType<DrawableOsuHitObject>())
            {
                adjustFadeIn(d.HitObject);
                foreach (var h in d.HitObject.NestedHitObjects.OfType<OsuHitObject>())
                    adjustFadeIn(h);
            }

            foreach (var drawable in drawables.Where((x, i) => i % 2 == 0))
                drawable.ApplyCustomUpdateState += ApplyHiddenState;
        }

        protected void ApplyHiddenState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableOsuHitObject d))
                return;

            var h = d.HitObject;

            var fadeOutStartTime = h.StartTime - h.TimePreempt + h.TimeFadeIn;
            var fadeOutDuration = h.TimePreempt * fade_out_duration_multiplier;

            // new duration from completed fade in to end (before fading out)
            var longFadeDuration = ((h as IHasEndTime)?.EndTime ?? h.StartTime) - fadeOutStartTime;

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
    }
}
