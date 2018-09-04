
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModReveal : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Reveal";
        public override string ShortenedName => "RV";
        public override FontAwesome Icon => FontAwesome.fa_blind;
        public override ModType Type => ModType.Fun;
        public override string Description => "Just let them fade in...";
        public override double ScoreMultiplier => 1;

        private const double fade_in_duration_multiplier = 0.4; // adopted from OsuModHidden

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables)
                drawable.ApplyCustomUpdateState += ApplyRevealState;
        }

        protected void ApplyRevealState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableOsuHitObject d))
                return;

            var h = d.HitObject;

            var fadeInStartTime = h.StartTime - h.TimePreempt;
            var fadeInDuration = h.TimePreempt * 0.9;
            var fadeOutStartTime = h.StartTime - h.TimePreempt + h.TimeFadeIn;  // for sliders
            var longFadeDuration = ((h as IHasEndTime)?.EndTime ?? h.StartTime) - fadeOutStartTime;

            // first prevent objects from being drawn, then let them fade in
            drawable.Hide();
            using (drawable.BeginAbsoluteSequence(fadeInStartTime, true))
                drawable.FadeIn(fadeInDuration, Easing.InCirc);

            // let sliders fade out midway for better visual clarity
            if (drawable is DrawableSlider slider)
            {
                using (slider.BeginAbsoluteSequence(fadeOutStartTime, true))
                    slider.Body.FadeOut(longFadeDuration, Easing.Out);
            }
        }
    }
}
