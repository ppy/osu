// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
using osu.Game.Audio;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class DrumRoll : TaikoHitObject, IHasEndTime
    {
        /// <summary>
        /// Drum roll distance that results in a duration of 1 speed-adjusted beat length.
        /// </summary>
        private const float base_distance = 100;

        public double EndTime => StartTime + Duration;

        public double Duration { get; set; }

        /// <summary>
        /// Numer of ticks per beat length.
        /// </summary>
        public int TickRate = 1;

        /// <summary>
        /// Number of drum roll ticks required for a "Good" hit.
        /// </summary>
        public double RequiredGoodHits { get; protected set; }

        /// <summary>
        /// Number of drum roll ticks required for a "Great" hit.
        /// </summary>
        public double RequiredGreatHits { get; protected set; }

        /// <summary>
        /// Total number of drum roll ticks.
        /// </summary>
        public int TotalTicks => Ticks.Count();

        /// <summary>
        /// Initializes the drum roll ticks if not initialized and returns them.
        /// </summary>
        public IEnumerable<DrumRollTick> Ticks => ticks ?? (ticks = createTicks());

        private List<DrumRollTick> ticks;

        /// <summary>
        /// The length (in milliseconds) between ticks of this drumroll.
        /// <para>Half of this value is the hit window of the ticks.</para>
        /// </summary>
        private double tickSpacing = 100;

        public override void ApplyDefaults(TimingInfo timing, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaults(timing, difficulty);

            tickSpacing = timing.BeatLengthAt(StartTime) / TickRate;

            RequiredGoodHits = TotalTicks * Math.Min(0.15, 0.05 + 0.10 / 6 * difficulty.OverallDifficulty);
            RequiredGreatHits = TotalTicks * Math.Min(0.30, 0.10 + 0.20 / 6 * difficulty.OverallDifficulty);
        }

        private List<DrumRollTick> createTicks()
        {
            var ret = new List<DrumRollTick>();

            if (tickSpacing == 0)
                return ret;

            bool first = true;
            for (double t = StartTime; t < EndTime + tickSpacing / 2; t += tickSpacing)
            {
                ret.Add(new DrumRollTick
                {
                    FirstTick = first,
                    ScrollTime = ScrollTime,
                    TickSpacing = tickSpacing,
                    StartTime = t,
                    IsStrong = IsStrong,
                    Samples = Samples.Select(s => new SampleInfo
                    {
                        Bank = s.Bank,
                        Name = @"slidertick",
                        Volume = s.Volume
                    }).ToList()
                });

                first = false;
            }

            return ret;
        }
    }
}