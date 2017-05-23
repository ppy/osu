using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class ControlPointInfo
    {
        /// <summary>
        /// All the control points.
        /// </summary>
        public readonly List<ControlPoint> ControlPoints = new List<ControlPoint>();

        /// <summary>
        /// Finds the difficulty control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the difficulty control point at.</param>
        /// <returns>The difficulty control point.</returns>
        public DifficultyControlPoint DifficultyPointAt(double time) =>
            ControlPoints.OfType<DifficultyControlPoint>().LastOrDefault(t => t.Time <= time) ?? new DifficultyControlPoint();

        /// <summary>
        /// Finds the effect control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the effect control point at.</param>
        /// <returns>The effect control point.</returns>
        public EffectControlPoint EffectPointAt(double time) =>
            ControlPoints.OfType<EffectControlPoint>().LastOrDefault(t => t.Time <= time) ?? new EffectControlPoint();

        /// <summary>
        /// Finds the sound control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the sound control point at.</param>
        /// <returns>The sound control point.</returns>
        public SoundControlPoint SoundPointAt(double time) =>
            ControlPoints.OfType<SoundControlPoint>().LastOrDefault(t => t.Time <= time) ?? new SoundControlPoint();

        /// <summary>
        /// Finds the timing control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the timing control point at.</param>
        /// <returns>The timing control point.</returns>
        public TimingControlPoint TimingPointAt(double time) =>
            ControlPoints.OfType<TimingControlPoint>().LastOrDefault(t => t.Time <= time) ?? new TimingControlPoint();

        /// <summary>
        /// Finds the maximum BPM represented by any timing control point.
        /// </summary>
        public double BPMMaximum =>
            60000 / (ControlPoints.OfType<TimingControlPoint>().OrderBy(c => c.BeatLength).FirstOrDefault() ?? new TimingControlPoint()).BeatLength;

        /// <summary>
        /// Finds the minimum BPM represented by any timing control point.
        /// </summary>
        public double BPMMinimum =>
            60000 / (ControlPoints.OfType<TimingControlPoint>().OrderByDescending(c => c.BeatLength).FirstOrDefault() ?? new TimingControlPoint()).BeatLength;

        /// <summary>
        /// Finds the mode BPM (most common BPM) represented by the control points.
        /// </summary>
        public double BPMMode =>
            60000 / (ControlPoints.OfType<TimingControlPoint>().GroupBy(c => c.BeatLength).OrderByDescending(grp => grp.Count()).FirstOrDefault()?.FirstOrDefault() ?? new TimingControlPoint()).BeatLength;
    }
}