// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Edit
{
    public interface IBeatSnapProvider
    {
        /// <summary>
        /// Snaps a duration to the closest beat of a timing point applicable at the reference time.
        /// </summary>
        /// <param name="referenceTime">The time of the timing point which <paramref name="duration"/> resides in.</param>
        /// <param name="duration">The duration to snap.</param>
        /// <param name="beatDivisor">The divisor to use for snapping purposes.</param>
        /// <returns>A value that represents <paramref name="duration"/> snapped to the closest beat of the timing point.</returns>
        double SnapTime(double referenceTime, double duration, int beatDivisor);

        /// <summary>
        /// Get the most appropriate beat length at a given time.
        /// </summary>
        /// <param name="referenceTime">A reference time used for lookup.</param>
        /// <param name="beatDivisor">The divisor to use for snapping purposes.</param>
        /// <returns>The most appropriate beat length.</returns>
        double GetBeatLengthAtTime(double referenceTime, int beatDivisor);

        /// <summary>
        /// Returns the current beat divisor.
        /// </summary>
        int BeatDivisor { get; }
    }
}
