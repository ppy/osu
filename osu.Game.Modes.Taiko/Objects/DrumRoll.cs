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
        public double TickDistance;

        public override void SetDefaultsFromBeatmap(Beatmap beatmap)
        {
            base.SetDefaultsFromBeatmap(beatmap);

            var baseDifficulty = beatmap.BeatmapInfo.BaseDifficulty;

            ControlPoint overridePoint;
            ControlPoint timingPoint = beatmap.TimingPointAt(StartTime, out overridePoint);
            var velocityAdjustment = overridePoint?.VelocityAdjustment ?? 1;
            var baseVelocity = 100 * baseDifficulty.SliderMultiplier / velocityAdjustment;

            Velocity = baseVelocity / timingPoint.BeatLength;
            TickDistance = baseVelocity / baseDifficulty.SliderTickRate;
        }

        public IEnumerable<DrumRollTick> Ticks
        {
            get
            {
                if (TickDistance == 0) yield break;

                int totalTicks = (int)(Length / TickDistance);

                for (int i = 0; i <= totalTicks; i++)
                {
                    double pos = (i * TickDistance);

                    yield return new DrumRollTick()
                    {
                        FirstTick = i == 0,
                        Colour = Colour,
                        StartTime = StartTime + pos / Length * Duration,
                        Scale = Scale,
                        Sample = new HitSampleInfo()
                        {
                            Type = SampleType.None,
                            Set = SampleSet.Soft
                        }
                    };
                }
            }
        }
    }
}
