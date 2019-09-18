// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModTimeRamp : Mod, IUpdatableByPlayfield, IApplicableToClock, IApplicableToBeatmap
    {
        /// <summary>
        /// The point in the beatmap at which the final ramping rate should be reached.
        /// </summary>
        private const double final_rate_progress = 0.75f;

        public override Type[] IncompatibleMods => new[] { typeof(ModTimeAdjust) };

        protected abstract double FinalRateAdjustment { get; }

        private double finalRateTime;
        private double beginRampTime;
        private IAdjustableClock clock;

        public virtual void ApplyToClock(IAdjustableClock clock)
        {
            this.clock = clock;

            lastAdjust = 1;

            // for preview purposes. during gameplay, Update will overwrite this setting.
            applyAdjustment(1);
        }

        public virtual void ApplyToBeatmap(IBeatmap beatmap)
        {
            HitObject lastObject = beatmap.HitObjects.LastOrDefault();

            beginRampTime = beatmap.HitObjects.FirstOrDefault()?.StartTime ?? 0;
            finalRateTime = final_rate_progress * ((lastObject as IHasEndTime)?.EndTime ?? lastObject?.StartTime ?? 0);
        }

        public virtual void Update(Playfield playfield)
        {
            applyAdjustment((clock.CurrentTime - beginRampTime) / finalRateTime);
        }

        private double lastAdjust = 1;

        /// <summary>
        /// Adjust the rate along the specified ramp
        /// </summary>
        /// <param name="amount">The amount of adjustment to apply (from 0..1).</param>
        private void applyAdjustment(double amount)
        {
            double adjust = 1 + (Math.Sign(FinalRateAdjustment) * MathHelper.Clamp(amount, 0, 1) * Math.Abs(FinalRateAdjustment));

            switch (clock)
            {
                case IHasPitchAdjust pitch:
                    pitch.PitchAdjust /= lastAdjust;
                    pitch.PitchAdjust *= adjust;
                    break;

                case IHasTempoAdjust tempo:
                    tempo.TempoAdjust /= lastAdjust;
                    tempo.TempoAdjust *= adjust;
                    break;

                default:
                    clock.Rate /= lastAdjust;
                    clock.Rate *= adjust;
                    break;
            }

            lastAdjust = adjust;
        }
    }
}
