// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Edit
{
    public interface IBeatSnapProvider
    {
        /// <summary>
        /// Snaps a duration to the closest beat of a timing point applicable at the reference time, factoring in the current <see cref="BeatDivisor"/>.
        /// </summary>
        /// <param name="time">The time to snap.</param>
        /// <param name="referenceTime">An optional reference point to use for timing point lookup.</param>
        /// <returns>A value that represents <paramref name="time"/> snapped to the closest beat of the timing point.</returns>
        double SnapTime(double time, double? referenceTime = null);

        /// <summary>
        /// Get the most appropriate beat length at a given time, pre-divided by <see cref="BeatDivisor"/>.
        /// </summary>
        /// <param name="referenceTime">A reference time used for lookup.</param>
        /// <returns>The most appropriate beat length, divided by <see cref="BeatDivisor"/>.</returns>
        double GetBeatLengthAtTime(double referenceTime);

        /// <summary>
        /// Returns the current beat divisor.
        /// </summary>
        int BeatDivisor { get; }
    }
}
