// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Audio.Track;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModTimeRamp : Mod, IUpdatableByPlayfield, IApplicableToTrack, IApplicableToBeatmap
    {
        /// <summary>
        /// The point in the beatmap at which the final ramping rate should be reached.
        /// </summary>
        private const double final_rate_progress = 0.75f;

        public override Type[] IncompatibleMods => new[] { typeof(ModTimeAdjust) };

        protected abstract double FinalRateAdjustment { get; }

        private double finalRateTime;
        private double beginRampTime;
        private Track track;

        public virtual void ApplyToTrack(Track track)
        {
            this.track = track;

            lastAdjust = 1;

            // for preview purposes. during gameplay, Update will overwrite this setting.
            applyAdjustment(1);
        }

        public virtual void ApplyToBeatmap(IBeatmap beatmap)
        {
            HitObject lastObject = beatmap.HitObjects.LastOrDefault();

            beginRampTime = beatmap.HitObjects.FirstOrDefault()?.StartTime ?? 0;
            finalRateTime = final_rate_progress * (lastObject?.GetEndTime() ?? 0);
        }

        public virtual void Update(Playfield playfield)
        {
            applyAdjustment((track.CurrentTime - beginRampTime) / finalRateTime);
        }

        private double lastAdjust = 1;

        /// <summary>
        /// Adjust the rate along the specified ramp
        /// </summary>
        /// <param name="amount">The amount of adjustment to apply (from 0..1).</param>
        private void applyAdjustment(double amount)
        {
            double adjust = 1 + (Math.Sign(FinalRateAdjustment) * Math.Clamp(amount, 0, 1) * Math.Abs(FinalRateAdjustment));

            track.Tempo.Value /= lastAdjust;
            track.Tempo.Value *= adjust;

            lastAdjust = adjust;
        }
    }
}
