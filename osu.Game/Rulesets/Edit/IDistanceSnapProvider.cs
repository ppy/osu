// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A snap provider which given a reference hit object and proposed distance from it, offers a more correct duration or distance value.
    /// </summary>
    [Cached]
    public interface IDistanceSnapProvider : IPositionSnapProvider
    {
        /// <summary>
        /// A multiplier which changes the ratio of distance travelled per time unit.
        /// Importantly, this is provided for manual usage, and not multiplied into any of the methods exposed by this interface.
        /// </summary>
        /// <seealso cref="BeatmapInfo.DistanceSpacing"/>
        IBindable<double> DistanceSpacingMultiplier { get; }

        /// <summary>
        /// The distance between two points within a timing point that are one beat length apart.
        /// </summary>
        float BeatSnapDistance { get; }

        /// <summary>
        /// Converts a duration to a distance without applying any snapping.
        /// </summary>
        /// <param name="referenceTime">A time to be used as a reference point for this operation.</param>
        /// <param name="duration">The duration to convert.</param>
        /// <returns>A value that represents <paramref name="duration"/> as a distance in the timing point.</returns>
        float DurationToDistance(double referenceTime, double duration);

        /// <summary>
        /// Converts a distance to a duration without applying any snapping.
        /// </summary>
        /// <param name="referenceTime">A time to be used as a reference point for this operation.</param>
        /// <param name="distance">The distance to convert.</param>
        /// <returns>A value that represents <paramref name="distance"/> as a duration in the timing point.</returns>
        double DistanceToDuration(double referenceTime, float distance);

        /// <summary>
        /// Given a distance from the provided hit object, find the valid snapped duration.
        /// </summary>
        /// <param name="referenceTime">A time to be used as a reference point for this operation.</param>
        /// <param name="distance">The distance to convert.</param>
        /// <returns>A value that represents <paramref name="distance"/> as a duration snapped to the closest beat of the timing point.</returns>
        double FindSnappedDuration(double referenceTime, float distance);

        /// <summary>
        /// Given a distance from the provided hit object, find the valid snapped distance.
        /// </summary>
        /// <param name="referenceTime">A time to be used as a reference point for this operation.</param>
        /// <param name="distance">The distance to convert.</param>
        /// <returns>
        /// A value that represents <paramref name="distance"/> snapped to the closest beat of the timing point.
        /// The distance will always be less than or equal to the provided <paramref name="distance"/>.
        /// </returns>
        float FindSnappedDistance(double referenceTime, float distance);
    }
}
