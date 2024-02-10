// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A snap provider which given a reference hit object and proposed distance from it, offers a more correct duration or distance value.
    /// </summary>
    [Cached]
    public interface IDistanceSnapProvider
    {
        /// <summary>
        /// A multiplier which changes the ratio of distance travelled per time unit.
        /// Importantly, this is provided for manual usage, and not multiplied into any of the methods exposed by this interface.
        /// </summary>
        /// <seealso cref="BeatmapInfo.DistanceSpacing"/>
        Bindable<double> DistanceSpacingMultiplier { get; }

        /// <summary>
        /// Retrieves the distance between two points within a timing point that are one beat length apart.
        /// </summary>
        /// <param name="referenceObject">An object to be used as a reference point for this operation.</param>
        /// <param name="useReferenceSliderVelocity">Whether the <paramref name="referenceObject"/>'s slider velocity should be factored into the returned distance.</param>
        /// <returns>The distance between two points residing in the timing point that are one beat length apart.</returns>
        float GetBeatSnapDistanceAt(HitObject referenceObject, bool useReferenceSliderVelocity = true);

        /// <summary>
        /// Converts a duration to a distance without applying any snapping.
        /// </summary>
        /// <param name="referenceObject">An object to be used as a reference point for this operation.</param>
        /// <param name="duration">The duration to convert.</param>
        /// <returns>A value that represents <paramref name="duration"/> as a distance in the timing point.</returns>
        float DurationToDistance(HitObject referenceObject, double duration);

        /// <summary>
        /// Converts a distance to a duration without applying any snapping.
        /// </summary>
        /// <param name="referenceObject">An object to be used as a reference point for this operation.</param>
        /// <param name="distance">The distance to convert.</param>
        /// <returns>A value that represents <paramref name="distance"/> as a duration in the timing point.</returns>
        double DistanceToDuration(HitObject referenceObject, float distance);

        /// <summary>
        /// Given a distance from the provided hit object, find the valid snapped duration.
        /// </summary>
        /// <param name="referenceObject">An object to be used as a reference point for this operation.</param>
        /// <param name="distance">The distance to convert.</param>
        /// <returns>A value that represents <paramref name="distance"/> as a duration snapped to the closest beat of the timing point.</returns>
        double FindSnappedDuration(HitObject referenceObject, float distance);

        /// <summary>
        /// Given a distance from the provided hit object, find the valid snapped distance.
        /// </summary>
        /// <param name="referenceObject">An object to be used as a reference point for this operation.</param>
        /// <param name="distance">The distance to convert.</param>
        /// <returns>
        /// A value that represents <paramref name="distance"/> snapped to the closest beat of the timing point.
        /// The distance will always be less than or equal to the provided <paramref name="distance"/>.
        /// </returns>
        float FindSnappedDistance(HitObject referenceObject, float distance);
    }
}
