// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModHidden : ModHidden, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Description => @"Play with fading fruits.";
        public override double ScoreMultiplier => 1.06;

        private const double fade_out_offset_multiplier = 0.6;
        private const double fade_out_duration_multiplier = 0.44;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.CatchFruitOnPlate = false;
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
            => ApplyNormalVisibilityState(hitObject, state);

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            if (!(hitObject is DrawableCatchHitObject catchDrawable))
                return;

            if (catchDrawable.NestedHitObjects.Any())
            {
                foreach (var nestedDrawable in catchDrawable.NestedHitObjects)
                {
                    if (nestedDrawable is DrawableCatchHitObject nestedCatchDrawable)
                        fadeOutHitObject(nestedCatchDrawable);
                }
            }
            else
                fadeOutHitObject(catchDrawable);
        }

        private void fadeOutHitObject(DrawableCatchHitObject drawable)
        {
            var hitObject = drawable.HitObject;

            double offset = hitObject.TimePreempt * fade_out_offset_multiplier;
            double duration = offset - hitObject.TimePreempt * fade_out_duration_multiplier;

            using (drawable.BeginAbsoluteSequence(hitObject.StartTime - offset))
                drawable.FadeOut(duration);
        }
    }
}
