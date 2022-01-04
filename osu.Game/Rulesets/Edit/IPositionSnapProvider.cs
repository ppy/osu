// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    public interface IPositionSnapProvider
    {
        /// <summary>
        /// Given a position, find a valid time and position snap.
        /// </summary>
        /// <remarks>
        /// This call should be equivalent to running <see cref="SnapScreenSpacePositionToValidPosition"/> with any additional logic that can be performed without the time immutability restriction.
        /// </remarks>
        /// <param name="screenSpacePosition">The screen-space position to be snapped.</param>
        /// <returns>The time and position post-snapping.</returns>
        SnapResult SnapScreenSpacePositionToValidTime(Vector2 screenSpacePosition);

        /// <summary>
        /// Given a position, find a value position snap, restricting time to its input value.
        /// </summary>
        /// <param name="screenSpacePosition">The screen-space position to be snapped.</param>
        /// <returns>The position post-snapping. Time will always be null.</returns>
        SnapResult SnapScreenSpacePositionToValidPosition(Vector2 screenSpacePosition);

        /// <summary>
        /// Retrieves the distance between two points within a timing point that are one beat length apart.
        /// </summary>
        /// <param name="referenceObject">An object to be used as a reference point for this operation.</param>
        /// <returns>The distance between two points residing in the timing point that are one beat length apart.</returns>
        float GetBeatSnapDistanceAt(HitObject referenceObject);

        /// <summary>
        /// Converts a duration to a distance.
        /// </summary>
        /// <param name="referenceObject">An object to be used as a reference point for this operation.</param>
        /// <param name="duration">The duration to convert.</param>
        /// <returns>A value that represents <paramref name="duration"/> as a distance in the timing point.</returns>
        float DurationToDistance(HitObject referenceObject, double duration);

        /// <summary>
        /// Converts a distance to a duration.
        /// </summary>
        /// <param name="referenceObject">An object to be used as a reference point for this operation.</param>
        /// <param name="distance">The distance to convert.</param>
        /// <returns>A value that represents <paramref name="distance"/> as a duration in the timing point.</returns>
        double DistanceToDuration(HitObject referenceObject, float distance);

        /// <summary>
        /// Converts a distance to a snapped duration.
        /// </summary>
        /// <param name="referenceObject">An object to be used as a reference point for this operation.</param>
        /// <param name="distance">The distance to convert.</param>
        /// <returns>A value that represents <paramref name="distance"/> as a duration snapped to the closest beat of the timing point.</returns>
        double GetSnappedDurationFromDistance(HitObject referenceObject, float distance);

        /// <summary>
        /// Converts an unsnapped distance to a snapped distance.
        /// The returned distance will always be floored (as to never exceed the provided <paramref name="distance"/>.
        /// </summary>
        /// <param name="referenceObject">An object to be used as a reference point for this operation.</param>
        /// <param name="distance">The distance to convert.</param>
        /// <returns>A value that represents <paramref name="distance"/> snapped to the closest beat of the timing point.</returns>
        float GetSnappedDistanceFromDistance(HitObject referenceObject, float distance);
    }
}
