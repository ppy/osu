// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModGrow : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Grow";

        public override string Acronym => "GR";

        public override IconUsage Icon => FontAwesome.Solid.ArrowsAltV;

        public override ModType Type => ModType.Fun;

        public override string Description => "Hit them at the right size!";

        public override double ScoreMultiplier => 1;

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables)
            {
                switch (drawable)
                {
                    case DrawableSpinner _:
                        continue;
                    default:
                        drawable.ApplyCustomUpdateState += ApplyCustomState;
                        break;
                }
            }
        }

        protected virtual void ApplyCustomState(DrawableHitObject drawable, ArmedState state)
        {
            var h = (OsuHitObject)drawable.HitObject;

            // apply grow effect
            switch (drawable)
            {
                case DrawableSliderHead _:
                case DrawableSliderTail _:
                    // special cases we should *not* be scaling.
                    break;
                case DrawableSlider _:
                case DrawableHitCircle _:
                {
                    using (drawable.BeginAbsoluteSequence(h.StartTime - h.TimePreempt, true))
                        drawable.ScaleTo(0.5f).Then().ScaleTo(1, h.TimePreempt, Easing.OutSine);
                    break;
                }
            }

            // remove approach circles
            switch (drawable)
            {
                case DrawableHitCircle circle:
                    // we don't want to see the approach circle
                    using (circle.BeginAbsoluteSequence(h.StartTime - h.TimePreempt, true))
                        circle.ApproachCircle.Hide();
                    break;
            }
        }
    }
}
