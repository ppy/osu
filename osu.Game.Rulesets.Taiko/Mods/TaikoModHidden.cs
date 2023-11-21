// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModHidden : ModHidden, IApplicableToDrawableRuleset<TaikoHitObject>
    {
        public override LocalisableString Description => @"Beats fade out before you hit them!";
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.06 : 1;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(TaikoModColorBlind)).ToArray();

        /// <summary>
        /// How far away from the hit target should hitobjects start to fade out.
        /// Range: [0, 1]
        /// </summary>
        private const float fade_out_start_time = 1f;

        /// <summary>
        /// How long hitobjects take to fade out, in terms of the scrolling length.
        /// Range: [0, 1]
        /// </summary>
        private const float fade_out_duration = 0.375f;

        private DrawableTaikoRuleset drawableRuleset = null!;

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            this.drawableRuleset = (DrawableTaikoRuleset)drawableRuleset;
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            ApplyNormalVisibilityState(hitObject, state);
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            switch (hitObject)
            {
                case DrawableDrumRollTick:
                case DrawableHit:
                    double preempt = drawableRuleset.TimeRange.Value / drawableRuleset.ControlPointAt(hitObject.HitObject.StartTime).Multiplier;
                    double start = hitObject.HitObject.StartTime - preempt * fade_out_start_time;
                    double duration = preempt * fade_out_duration;

                    using (hitObject.BeginAbsoluteSequence(start))
                    {
                        hitObject.FadeOut(duration);

                        // DrawableHitObject sets LifetimeEnd to LatestTransformEndTime if it isn't manually changed.
                        // in order for the object to not be killed before its actual end time (as the latest transform ends earlier), set lifetime end explicitly.
                        hitObject.LifetimeEnd = state == ArmedState.Idle || !hitObject.AllJudged
                            ? hitObject.HitObject.GetEndTime() + hitObject.HitObject.HitWindows.WindowFor(HitResult.Miss)
                            : hitObject.HitStateUpdateTime;
                        // extend the lifetime end of the object in order to allow its nested strong hit (if any) to be judged.
                        hitObject.LifetimeEnd += DrawableHit.StrongNestedHit.SECOND_HIT_WINDOW;
                    }

                    break;
            }
        }
    }
}
