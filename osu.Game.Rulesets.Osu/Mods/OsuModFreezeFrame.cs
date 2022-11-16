// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModFreezeFrame : Mod, IApplicableToDrawableHitObject, IApplicableToBeatmap, ICanBeToggledDuringReplay
    {
        public override string Name => "Freeze Frame";

        public override string Acronym => "FR";

        public override double ScoreMultiplier => 1;

        public override LocalisableString Description => "Burn the notes into your memory.";

        //Alters the transforms of the approach circles, breaking the effects of these mods.
        public override Type[] IncompatibleMods => new[] { typeof(OsuModApproachDifferent) };

        public override ModType Type => ModType.Fun;

        //mod breaks normal approach circle preempt
        private double originalPreempt;

        public BindableBool IsDisabled { get; } = new BindableBool();

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var firstHitObject = beatmap.HitObjects.OfType<OsuHitObject>().FirstOrDefault();
            if (firstHitObject == null)
                return;

            originalPreempt = firstHitObject.TimePreempt;

            IsDisabled.BindValueChanged(s =>
            {
                if (s.NewValue)
                {
                    foreach (var obj in beatmap.HitObjects.OfType<OsuHitObject>())
                    {
                        applyFadeInAdjustment(obj);
                    }
                }
                else
                {
                    calculateComboTime();
                }
            }, true);

            void calculateComboTime()
            {
                double lastNewComboTime = 0;

                foreach (var obj in beatmap.HitObjects.OfType<OsuHitObject>())
                {
                    if (obj.NewCombo) { lastNewComboTime = obj.StartTime; }

                    applyFadeInAdjustment(obj, lastNewComboTime);
                }
            }

            void applyFadeInAdjustment(OsuHitObject osuObject, double? lastNewComboTime = null)
            {
                if (lastNewComboTime != null)
                {
                    osuObject.TimePreempt += osuObject.StartTime - lastNewComboTime.Value;
                }
                else
                {
                    osuObject.TimePreempt = 600;
                }

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
                            applyFadeInAdjustment(nested, lastNewComboTime);
                            break;
                    }
                }
            }
        }

        public void ApplyToDrawableHitObject(DrawableHitObject drawableObject)
        {
            drawableObject.ApplyCustomUpdateState += (drawableHitObject, _) =>
            {
                if (IsDisabled.Value) return;

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
