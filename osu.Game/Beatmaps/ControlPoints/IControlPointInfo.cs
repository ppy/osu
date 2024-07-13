// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;

namespace osu.Game.Beatmaps.ControlPoints
{
    public interface IControlPointInfo
    {
        /// <summary>
        /// All control points grouped by time.
        /// </summary>
        IBindableList<ControlPointGroup> Groups { get; }

        /// <summary>
        /// All timing points.
        /// </summary>
        IReadOnlyList<TimingControlPoint> TimingPoints { get; }

        /// <summary>
        /// All effect points.
        /// </summary>
        IReadOnlyList<EffectControlPoint> EffectPoints { get; }

        /// <summary>
        /// All control points, of all types.
        /// </summary>
        IEnumerable<ControlPoint> AllControlPoints { get; }

        /// <summary>
        /// Finds the effect control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the effect control point at.</param>
        /// <returns>The effect control point.</returns>
        EffectControlPoint EffectPointAt(double time);

        /// <summary>
        /// Finds the timing control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the timing control point at.</param>
        /// <returns>The timing control point.</returns>
        TimingControlPoint TimingPointAt(double time);

        /// <summary>
        /// Finds the maximum BPM represented by any timing control point.
        /// </summary>
        double BPMMaximum { get; }

        /// <summary>
        /// Finds the minimum BPM represented by any timing control point.
        /// </summary>
        double BPMMinimum { get; }

        /// <summary>
        /// Returns the time on the given beat divisor closest to the given time.
        /// </summary>
        /// <param name="time">The time to find the closest snapped time to.</param>
        /// <param name="beatDivisor">The beat divisor to snap to.</param>
        /// <param name="referenceTime">An optional reference point to use for timing point lookup.</param>
        double GetClosestSnappedTime(double time, int beatDivisor, double? referenceTime = null);

        /// <summary>
        /// Returns the time on *ANY* valid beat divisor, favouring the divisor closest to the given time.
        /// </summary>
        /// <param name="time">The time to find the closest snapped time to.</param>
        double GetClosestSnappedTime(double time);

        /// <summary>
        /// Returns the beat snap divisor closest to the given time. If two are equally close, the smallest divisor is returned.
        /// </summary>
        /// <param name="time">The time to find the closest beat snap divisor to.</param>
        /// <param name="referenceTime">An optional reference point to use for timing point lookup.</param>
        int GetClosestBeatDivisor(double time, double? referenceTime = null);
    }
}
