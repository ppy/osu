// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Samples;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Modes.Taiko.Objects
{
    public class DrumRoll : TaikoHitObject
    {
        public override double EndTime => StartTime + Length / Velocity;

        /// <summary>
        /// Raw length of the drum roll in distance units.
        /// </summary>
        public double Length;

        /// <summary>
        /// Velocity of the drum roll in distance units.
        /// </summary>
        public double Velocity;

        /// <summary>
        /// The distance between ticks of this drumroll.
        /// <para>Half of this value is the hit window of the ticks.</para>
        /// </summary>
        public double TickTimeDistance;

        /// <summary>
        /// Number of drum roll ticks required for a "Good" hit.
        /// </summary>
        public double RequiredGoodHits;

        /// <summary>
        /// Number of drum roll ticks required for a "Great" hit.
        /// </summary>
        public double RequiredGreatHits;

        /// <summary>
        /// Total number of drum roll ticks.
        /// </summary>
        public int TotalTicks;

        /// <summary>
        /// Initializes the drum roll ticks if not initialized and returns them.
        /// </summary>
        public IEnumerable<DrumRollTick> Ticks
        {
            get
            {
                if (ticks == null)
                    createTicks();
                return ticks;
            }
        }

        private List<DrumRollTick> ticks;

        public override void SetDefaultsFromBeatmap(Beatmap beatmap)
        {
            base.SetDefaultsFromBeatmap(beatmap);

            Velocity = beatmap.SliderVelocityAt(StartTime) / 1000;
            TickTimeDistance = beatmap.Timing.BeatLengthAt(StartTime);

            if (beatmap.BeatmapInfo.BaseDifficulty.SliderTickRate == 3)
                TickTimeDistance /= 3;
            else
                TickTimeDistance /= 4;

            TotalTicks = Ticks.Count();
            RequiredGoodHits = TotalTicks * Math.Min(0.15, 0.05 + 0.10 / 6 * beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty);
            RequiredGreatHits = TotalTicks * Math.Min(0.30, 0.10 + 0.20 / 6 * beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty);
        }

        private void createTicks()
        {
            ticks = new List<DrumRollTick>();

            if (TickTimeDistance == 0)
                return;

            bool first = true;
            for (double t = StartTime; t < EndTime + (int)TickTimeDistance; t += TickTimeDistance)
            {
                ticks.Add(new DrumRollTick
                {
                    FirstTick = first,
                    PreEmpt = PreEmpt,
                    TickTimeDistance = TickTimeDistance,
                    Colour = Colour,
                    StartTime = t,
                    Sample = new HitSampleInfo
                    {
                        Type = SampleType.None,
                        Set = SampleSet.Soft
                    }
                });

                first = false;
            }
        }

        public override TaikoHitType Type
        {
            get
            {
                SampleType st = Sample?.Type ?? SampleType.None;

                return TaikoHitType.DrumRoll | ((st & SampleType.Finish) > 0 ? TaikoHitType.Finisher : TaikoHitType.None);
            }
        }
    }
}
