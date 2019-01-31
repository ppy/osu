// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

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
                if (drawable is DrawableSpinner spinner)
                    return;
                
                drawable.ApplyCustomUpdateState += applyCustomState;
            }
        }

        protected virtual void applyCustomState(DrawableHitObject drawable, ArmedState state)
        {   
            var hitObject = (OsuHitObject) drawable.HitObject;

            double appearTime = hitObject.StartTime - hitObject.TimePreempt - 1;
            double scaleDuration = hitObject.TimePreempt + 1;

            var originalScale = drawable.Scale;
            drawable.Scale /= 2;

            using (drawable.BeginAbsoluteSequence(appearTime, true))
                    drawable.ScaleTo(originalScale, scaleDuration, Easing.OutSine);

            if (drawable is DrawableHitCircle circle)
                using (circle.BeginAbsoluteSequence(hitObject.StartTime - hitObject.TimePreempt))
                    circle.ApproachCircle.Hide();
        }
    }
}