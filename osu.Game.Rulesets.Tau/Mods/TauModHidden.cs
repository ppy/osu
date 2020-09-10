using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Tau.Objects;
using osu.Game.Rulesets.Tau.Objects.Drawables;

namespace osu.Game.Rulesets.Tau.Mods
{
    public class TauModHidden : ModHidden
    {
        public override string Description => @"Play with no beats and fading sliders.";
        public override double ScoreMultiplier => 1.06;

        public override Type[] IncompatibleMods => new[] { typeof(TauModAutoHold) };

        private const double fadeInDurationMultiplier = 0.4;
        private const double fadeOutDurationMultiplier = 0.3;

        public override void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            static void adjustFadeIn(TauHitObject h) => h.TimeFadeIn = h.TimePreempt * fadeInDurationMultiplier;

            foreach (var d in drawables.OfType<DrawableTauHitObject>())
            {
                adjustFadeIn(d.HitObject);

                foreach (var h in d.HitObject.NestedHitObjects.OfType<TauHitObject>())
                    adjustFadeIn(h); // future proofing
            }

            base.ApplyToDrawableHitObjects(drawables);
        }

        protected override void ApplyHiddenState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableTauHitObject d))
                return;

            var h = d.HitObject;

            var fadeOutStartTime = h.StartTime - h.TimePreempt + h.TimeFadeIn;
            var fadeOutDuration = h.TimePreempt * fadeOutDurationMultiplier;

            // future proofing yet again.
            switch (drawable)
            {
                case DrawableTauHitObject beat:
                    using (drawable.BeginAbsoluteSequence(fadeOutStartTime, true))
                        beat.FadeOut(fadeOutDuration);

                    break;
            }

            base.ApplyHiddenState(drawable, state);
        }
    }
}
