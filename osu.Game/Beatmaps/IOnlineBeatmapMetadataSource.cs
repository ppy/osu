// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Unifying interface for sources of <see cref="OnlineBeatmapMetadata"/>.
    /// </summary>
    public interface IOnlineBeatmapMetadataSource : IDisposable
    {
        /// <summary>
        /// Whether this source can currently service lookups.
        /// </summary>
        bool Available { get; }

        /// <summary>
        /// Looks up the online metadata for the supplied <paramref name="beatmapInfo"/>.
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to look up.</param>
        /// <param name="onlineMetadata">
        /// An <see cref="OnlineBeatmapMetadata"/> instance if the lookup is successful.
        /// <see langword="null"/> if a mismatch between the local instance and the looked-up data was detected.
        /// The returned value is only valid if the return value of the method is <see langword="true"/>.
        /// </param>
        /// <returns>
        /// Whether the lookup was performed.
        /// </returns>
        bool TryLookup(BeatmapInfo beatmapInfo, out OnlineBeatmapMetadata? onlineMetadata);
    }
}
