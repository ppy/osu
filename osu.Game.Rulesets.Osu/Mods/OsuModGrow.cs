// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics;
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

        public override FontAwesome Icon => FontAwesome.fa_arrows_v;

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

            var scale = drawable.Scale;
            using (drawable.BeginAbsoluteSequence(h.StartTime - h.TimePreempt, true))
                drawable.ScaleTo(scale / 2).Then().ScaleTo(scale, h.TimePreempt, Easing.OutSine);

            switch (drawable)
            {
                case DrawableHitCircle circle:
                {
                    // we don't want to see the approach circle
                    using (circle.BeginAbsoluteSequence(h.StartTime - h.TimePreempt, true))
                        circle.ApproachCircle.Hide();
                    break;
                }
            }
        }
    }
}
