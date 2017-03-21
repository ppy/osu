using osu.Game.Beatmaps.Timing;
using osu.Game.Database;

namespace osu.Game.Modes.Taiko.Objects
{
    public class BarLine
    {
        /// <summary>
        /// The start time of the control point this bar line represents.
        /// </summary>
        public double StartTime;

        /// <summary>
        /// The time to scroll in the bar line.
        /// </summary>
        public double PreEmpt;

        /// <summary>
        /// Whether this is a major bar line (affects display).
        /// </summary>
        public bool IsMajor;

        public void ApplyDefaults(TimingInfo timing, BeatmapDifficulty difficulty)
        {
            PreEmpt = 600 / (timing.SliderVelocityAt(StartTime) * difficulty.SliderMultiplier) * 1000;
        }
    }
}
