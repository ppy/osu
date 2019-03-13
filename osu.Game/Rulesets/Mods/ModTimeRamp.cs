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
    public abstract class ModTimeRamp : Mod
    {
        public override Type[] IncompatibleMods => new[] { typeof(ModTimeAdjust) };

        protected abstract double FinalRateAdjustment { get; }
    }

    public abstract class ModTimeRamp<T> : ModTimeRamp, IUpdatableByPlayfield, IApplicableToClock, IApplicableToBeatmap<T>
        where T : HitObject
    {
        private double finalRateTime;

        private double beginRampTime;

        private IAdjustableClock clock;

        /// <summary>
        /// The point in the beatmap at which the final ramping rate should be reached.
        /// </summary>
        private const double final_rate_progress = 0.75f;

        /// <summary>
        /// The adjustment applied on entering this mod's application method.
        /// </summary>
        private double baseAdjust;

        public virtual void ApplyToClock(IAdjustableClock clock)
        {
            this.clock = clock;

            // we capture the adjustment applied before entering our application method.
            // this will cover external changes, which should re-fire this method.
            baseAdjust = (clock as IHasPitchAdjust)?.PitchAdjust ?? clock.Rate;

            // for preview purposes. during gameplay, Update will overwrite this setting.
            applyAdjustment(1);
        }

        public virtual void ApplyToBeatmap(Beatmap<T> beatmap)
        {
            HitObject lastObject = beatmap.HitObjects.LastOrDefault();

            beginRampTime = beatmap.HitObjects.FirstOrDefault()?.StartTime ?? 0;
            finalRateTime = final_rate_progress * ((lastObject as IHasEndTime)?.EndTime ?? lastObject?.StartTime ?? 0);
        }

        public virtual void Update(Playfield playfield)
        {
            var absRate = Math.Abs(FinalRateAdjustment);
            applyAdjustment(MathHelper.Clamp(absRate * ((clock.CurrentTime - beginRampTime) / finalRateTime), 0, absRate));
        }

        private void applyAdjustment(double adjustment)
        {
            var localAdjust = 1 + Math.Sign(FinalRateAdjustment) * adjustment;

            if (clock is IHasPitchAdjust tempo)
                tempo.PitchAdjust = baseAdjust * localAdjust;
            else
                clock.Rate = baseAdjust * localAdjust;
        }
    }
}
