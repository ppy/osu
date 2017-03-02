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
        /// Number of slider ticks required for a "Good" hit.
        /// </summary>
        public int RequiredGoodHits;

        /// <summary>
        /// Number of slider ticks required for a "Great" hit.
        /// </summary>
        public int RequiredGreatHits;

        public override void SetDefaultsFromBeatmap(Beatmap beatmap)
        {
            base.SetDefaultsFromBeatmap(beatmap);

            // Don't ask... Old osu! had a random multiplier here, that we now have to multiply everywhere
            float fudgeFactor = 1.4f;

            Length *= fudgeFactor;

            var baseDifficulty = beatmap.BeatmapInfo.BaseDifficulty;

            ControlPoint overridePoint;
            ControlPoint timingPoint = beatmap.TimingPointAt(StartTime, out overridePoint);
            var velocityAdjustment = overridePoint?.VelocityAdjustment ?? 1;
            var baseVelocity = 100 * baseDifficulty.SliderMultiplier * fudgeFactor / velocityAdjustment;

            Velocity = baseVelocity / timingPoint.BeatLength;

            TickTimeDistance = timingPoint.BeatLength;

            if (baseDifficulty.SliderTickRate == 3)
                TickTimeDistance /= 3;
            else
                TickTimeDistance /= 4;

            int totalTicks = Ticks.Count();
            RequiredGoodHits = (int)(totalTicks * Math.Min(0.15, 0.05 + (0.10 / 6) * beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty));
            RequiredGreatHits = (int)(totalTicks * Math.Min(0.30, 0.10 + (0.20 / 6) * beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty));
        }

        public IEnumerable<DrumRollTick> Ticks
        {
            get
            {
                if (TickTimeDistance == 0) yield break;

                bool first = true;
                for (double t = StartTime; t < EndTime; t += TickTimeDistance)
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
    }
}
