// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Scoring;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModClassic : ModClassic, IApplicableToHitObject, IApplicableToDrawableRuleset<TaikoHitObject>, IApplicableToDrawableHitObject
    {
        public void ApplyToHitObject(HitObject hitObject)
        {
            foreach (var ho in hitObject.EnumerateAllHitObjects())
            {
                if (ho.HitWindows is TaikoHitWindows taikoHitWindows)
                    taikoHitWindows.ClassicModActive = true;
            }
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            var drawableTaikoRuleset = (DrawableTaikoRuleset)drawableRuleset;
            drawableTaikoRuleset.LockPlayfieldAspectRange.Value = false;
        }

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (drawable is DrawableTaikoHitObject hit)
                hit.SnapJudgementLocation = true;
        }
    }
}
