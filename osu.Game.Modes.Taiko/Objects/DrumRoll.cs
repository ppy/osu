// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Samples;
using osu.Game.Modes.Objects.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.Timing;
using osu.Game.Database;

namespace osu.Game.Modes.Taiko.Objects
{
    public class DrumRoll : TaikoHitObject, IHasDistance
    {
        /// <summary>
        /// Drum roll distance that results in a duration of 1 speed-adjusted beat length.
        /// </summary>
        private const float base_distance = 100;

        public double EndTime => StartTime + Distance / Velocity;

        public double Duration => EndTime - StartTime;

        /// <summary>
        /// Raw length of the drum roll in positional length units.
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// Velocity of the drum roll in positional length units per millisecond.
        /// </summary>
        public double Velocity { get; protected set; } = 5;

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

            double speedAdjutedBeatLength = timing.SpeedMultiplierAt(StartTime) * timing.BeatLengthAt(StartTime);

            Velocity = base_distance * difficulty.SliderMultiplier / speedAdjutedBeatLength * VelocityMultiplier;
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
            for (double t = StartTime; t < EndTime + (int)tickSpacing; t += tickSpacing)
            {
                ret.Add(new DrumRollTick
                {
                    FirstTick = first,
                    PreEmpt = PreEmpt,
                    TickSpacing = tickSpacing,
                    StartTime = t,
                    IsStrong = IsStrong,
                    Sample = new HitSampleInfo
                    {
                        Type = SampleType.None,
                        Set = SampleSet.Soft
                    }
                });

                first = false;
            }

            return ret;
        }
    }
}