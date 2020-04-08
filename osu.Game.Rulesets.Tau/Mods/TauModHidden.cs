// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Tau.Objects;
using osu.Game.Rulesets.Tau.Objects.Drawables;

namespace osu.Game.Rulesets.Tau.Mods
{
    public class TauModHidden : ModHidden
    {
        public override string Description => @"Play with no beats and fading sliders.";
        public override double ScoreMultiplier => 1.06;

        public override Type[] IncompatibleMods => new[] { typeof(TauModAutoHold) };

        private const double fade_in_duration_multiplier = 0.4;
        private const double fade_out_duration_multiplier = 0.3;

        public override void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            static void adjustFadeIn(TauHitObject h) => h.TimeFadeIn = h.TimePreempt * fade_in_duration_multiplier;

            foreach (var d in drawables.OfType<DrawabletauHitObject>())
            {
                adjustFadeIn(d.HitObject);

                foreach (var h in d.HitObject.NestedHitObjects.OfType<TauHitObject>())
                    adjustFadeIn(h); // future proofing
            }

            base.ApplyToDrawableHitObjects(drawables);
        }

        protected override void ApplyHiddenState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawabletauHitObject d))
                return;

            var h = d.HitObject;

            var fadeOutStartTime = h.StartTime - h.TimePreempt + h.TimeFadeIn;
            var fadeOutDuration = h.TimePreempt * fade_out_duration_multiplier;

            // new duration from completed fade in to end (before fading out)
            var longFadeDuration = (h as IHasEndTime)?.EndTime ?? h.StartTime - fadeOutStartTime;

            // future proofing yet again.
            switch (drawable)
            {
                case DrawabletauHitObject beat:
                    using (drawable.BeginAbsoluteSequence(fadeOutStartTime, true))
                        beat.FadeOut(fadeOutDuration);

                    break;
            }

            base.ApplyHiddenState(drawable, state);
        }
    }
}
