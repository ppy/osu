// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Unifying interface for sources of online beatmap metadata.
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
        /// <returns>
        /// An <see cref="OnlineBeatmapMetadata"/> instance if the lookup is successful, or <see langword="null"/> if the lookup did not return a matching beatmap.
        /// </returns>
        OnlineBeatmapMetadata? Lookup(BeatmapInfo beatmapInfo);
    }
}
