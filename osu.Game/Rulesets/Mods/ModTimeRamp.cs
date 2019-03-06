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
        public override Type[] IncompatibleMods => new[] { typeof(ModDoubleTime), typeof(ModHalfTime) };

        protected abstract double FinalRateAdjustment { get; }
    }

    public abstract class ModTimeRamp<T> : ModTimeRamp, IUpdatableByPlayfield, IApplicableToClock, IApplicableToBeatmap<T>
        where T : HitObject
    {
        private double finalRateTime;

        private double beginRampTime;

        private IAdjustableClock clock;

        private IHasPitchAdjust pitchAdjust;

        /// <summary>
        /// The point in the beatmap at which the final ramping rate should be reached.
        /// </summary>
        private const double final_rate_progress = 0.75f;

        public virtual void ApplyToClock(IAdjustableClock clock)
        {
            this.clock = clock;
            pitchAdjust = (IHasPitchAdjust)clock;

            // for preview purposes
            pitchAdjust.PitchAdjust = 1.0 + FinalRateAdjustment;
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
            var adjustment = MathHelper.Clamp(absRate * ((clock.CurrentTime - beginRampTime) / finalRateTime), 0, absRate);

            pitchAdjust.PitchAdjust = 1 + Math.Sign(FinalRateAdjustment) * adjustment;
        }
    }
}
