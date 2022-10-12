// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModFreezeFrame : Mod, IApplicableToBeatmap, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Freeze Frame";

        public override string Acronym => "FR";

        public override double ScoreMultiplier => 1;

        public override LocalisableString Description => "Burn the notes into your memory.";

        public override ModType Type => ModType.Fun;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            (drawableRuleset.Playfield as OsuPlayfield)?.FollowPoints.Hide();
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            double lastNewComboTime = 0;

            foreach (var obj in beatmap.HitObjects.OfType<OsuHitObject>())
            {
                if (obj.NewCombo) { lastNewComboTime = obj.StartTime; }

                applyFadeInAdjustment(obj);
            }

            void applyFadeInAdjustment(OsuHitObject osuObject)
            {
                osuObject.TimePreempt += osuObject.StartTime - lastNewComboTime;

                foreach (var nested in osuObject.NestedHitObjects.OfType<OsuHitObject>())
                {
                    switch (nested)
                    {
                        //SliderRepeat wont layer correctly if preempt is changed.
                        case SliderRepeat:
                            break;

                        default:
                            applyFadeInAdjustment(nested);
                            break;
                    }
                }
            }
        }
    }
}
