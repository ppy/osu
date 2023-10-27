// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModFreezeFrame : Mod, IApplicableToDrawableHitObject, IApplicableToBeatmap
    {
        public override string Name => "Freeze Frame";

        public override string Acronym => "FR";

        public override double ScoreMultiplier => 1;

        public override LocalisableString Description => "Burn the notes into your memory.";

        //Alters the transforms of the approach circles, breaking the effects of these mods.
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModApproachDifferent), typeof(OsuModTransform) }).ToArray(); 

        public override ModType Type => ModType.Fun;

        //mod breaks normal approach circle preempt
        private double originalPreempt;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var firstHitObject = beatmap.HitObjects.OfType<OsuHitObject>().FirstOrDefault();
            if (firstHitObject == null)
                return;

            double lastNewComboTime = 0;

            originalPreempt = firstHitObject.TimePreempt;

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
                        //Freezing the SliderTicks doesnt play well with snaking sliders
                        case SliderTick:
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

        public void ApplyToDrawableHitObject(DrawableHitObject drawableObject)
        {
            drawableObject.ApplyCustomUpdateState += (drawableHitObject, _) =>
            {
                if (drawableHitObject is not DrawableHitCircle drawableHitCircle) return;

                var hitCircle = drawableHitCircle.HitObject;
                var approachCircle = drawableHitCircle.ApproachCircle;

                // Reapply scale, ensuring the AR isn't changed due to the new preempt.
                approachCircle.ClearTransforms(targetMember: nameof(approachCircle.Scale));
                approachCircle.ScaleTo(4 * (float)(hitCircle.TimePreempt / originalPreempt));

                using (drawableHitCircle.ApproachCircle.BeginAbsoluteSequence(hitCircle.StartTime - hitCircle.TimePreempt))
                    approachCircle.ScaleTo(1, hitCircle.TimePreempt).Then().Expire();
            };
        }
    }
}
