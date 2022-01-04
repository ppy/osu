// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Database;

#nullable enable

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A representation of a collection of beatmap difficulties, generally packaged as an ".osz" archive.
    /// </summary>
    public interface IBeatmapSetInfo : IHasOnlineID<int>, IEquatable<IBeatmapSetInfo>, IHasNamedFiles
    {
        /// <summary>
        /// The date when this beatmap was imported.
        /// </summary>
        DateTimeOffset DateAdded { get; }

        /// <summary>
        /// The best-effort metadata representing this set. In the case metadata differs between contained beatmaps, one is arbitrarily chosen.
        /// </summary>
        IBeatmapMetadataInfo Metadata { get; }

        /// <summary>
        /// All beatmaps contained in this set.
        /// </summary>
        IEnumerable<IBeatmapInfo> Beatmaps { get; }

        /// <summary>
        /// The maximum star difficulty of all beatmaps in this set.
        /// </summary>
        double MaxStarDifficulty { get; }

        /// <summary>
        /// The maximum playable length in milliseconds of all beatmaps in this set.
        /// </summary>
        double MaxLength { get; }

        /// <summary>
        /// The maximum BPM of all beatmaps in this set.
        /// </summary>
        double MaxBPM { get; }
    }
}
