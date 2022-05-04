// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Edit
{
    public interface IDistanceSnapProvider : IPositionSnapProvider
    {
        /// <summary>
        /// The spacing multiplier applied to beat snap distances.
        /// </summary>
        /// <seealso cref="BeatmapInfo.DistanceSpacing"/>
        IBindable<double> DistanceSpacingMultiplier { get; }

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
