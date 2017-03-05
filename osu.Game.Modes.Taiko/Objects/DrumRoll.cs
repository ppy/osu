// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Modes.Osu.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Beatmaps.Samples;

namespace osu.Game.Modes.Taiko.Objects
{
    public class DrumRoll : TaikoHitObject
    {
        public override double EndTime => StartTime + RepeatCount * Length / Velocity;

        public double Length;
        public int RepeatCount = 1;

        public double Velocity;
        public double TickTimeDistance;

        /// <summary>
        /// Number of drum roll ticks required for a "Good" hit.
        /// </summary>
        public int RequiredGoodHits;

        /// <summary>
        /// Number of drum roll ticks required for a "Great" hit.
        /// </summary>
        public int RequiredGreatHits;

        /// <summary>
        /// Total number of drum roll ticks.
        /// </summary>
        public int TotalTicks;

        public override void SetDefaultsFromBeatmap(Beatmap beatmap)
        {
            base.SetDefaultsFromBeatmap(beatmap);

            Length *= SLIDER_FUDGE_FACTOR;

            var baseDifficulty = beatmap.BeatmapInfo.BaseDifficulty;

            ControlPoint overridePoint;
            ControlPoint timingPoint = beatmap.TimingPointAt(StartTime, out overridePoint);
            var velocityAdjustment = overridePoint?.VelocityAdjustment ?? 1;
            var baseVelocity = 100 * baseDifficulty.SliderMultiplier * SLIDER_FUDGE_FACTOR / velocityAdjustment;

            Velocity = baseVelocity / timingPoint.BeatLength;

            TickTimeDistance = timingPoint.BeatLength;

            if (baseDifficulty.SliderTickRate == 3)
                TickTimeDistance /= 3;
            else
                TickTimeDistance /= 4;

            TotalTicks = Ticks.Count();
            RequiredGoodHits = (int)(TotalTicks * Math.Min(0.15, 0.05 + (0.10 / 6) * beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty));
            RequiredGreatHits = (int)(TotalTicks * Math.Min(0.30, 0.10 + (0.20 / 6) * beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty));
        }

        public IEnumerable<DrumRollTick> Ticks
        {
            get
            {
                if (TickTimeDistance == 0) yield break;

                bool first = true;
                for (double t = StartTime; t < EndTime + TickTimeDistance; t += TickTimeDistance)
                {
                    yield return new DrumRollTick()
                    {
                        FirstTick = first,
                        PreEmpt = PreEmpt,
                        TickTimeDistance = TickTimeDistance,
                        Colour = Colour,
                        StartTime = t,
                        Scale = Scale,
                        Sample = new HitSampleInfo()
                        {
                            Type = SampleType.None,
                            Set = SampleSet.Soft
                        }
                    };

                    first = false;
                }
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
