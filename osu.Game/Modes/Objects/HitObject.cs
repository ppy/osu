// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Samples;
using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
using System.Collections.Generic;

namespace osu.Game.Modes.Objects
{
    /// <summary>
    /// A HitObject describes an object in a Beatmap.
    /// <para>
    /// HitObjects may contain more properties for which you should be checking through the IHas* types.
    /// </para>
    /// </summary>
    public class HitObject
    {
        /// <summary>
        /// The time at which the HitObject starts.
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// The sample banks to be played when this hit object is hit.
        /// </summary>
        public List<SampleBank> SampleBanks = new List<SampleBank>();

        /// <summary>
        /// Applies default values to this HitObject.
        /// </summary>
        /// <param name="difficulty">The difficulty settings to use.</param>
        /// <param name="timing">The timing settings to use.</param>
        public virtual void ApplyDefaults(TimingInfo timing, BeatmapDifficulty difficulty)
        {
            foreach (var bank in SampleBanks)
            {
                if (!string.IsNullOrEmpty(bank.Name))
                    continue;

                // If the bank is not assigned a name, assign it from the relevant timing point
                ControlPoint overridePoint;
                ControlPoint timingPoint = timing.TimingPointAt(StartTime, out overridePoint);

                bank.Name = (overridePoint ?? timingPoint)?.SampleBank.Name ?? string.Empty;
                bank.Volume = (overridePoint ?? timingPoint)?.SampleBank.Volume ?? 0;
            }
        }
    }
}
