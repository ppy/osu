// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Database;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles all processing required to ensure a local beatmap is in a consistent state with any changes.
    /// </summary>
    public interface IBeatmapUpdater : IDisposable
    {
        /// <summary>
        /// Queue a beatmap for background processing.
        /// </summary>
        /// <param name="beatmapSet">The managed beatmap set to update. A transaction will be opened to apply changes.</param>
        /// <param name="lookupScope">The preferred scope to use for metadata lookup.</param>
        void Queue(Live<BeatmapSetInfo> beatmapSet, MetadataLookupScope lookupScope = MetadataLookupScope.LocalCacheFirst);

        /// <summary>
        /// Run all processing on a beatmap immediately.
        /// </summary>
        /// <param name="beatmapSet">The managed beatmap set to update. A transaction will be opened to apply changes.</param>
        /// <param name="lookupScope">The preferred scope to use for metadata lookup.</param>
        void Process(BeatmapSetInfo beatmapSet, MetadataLookupScope lookupScope = MetadataLookupScope.LocalCacheFirst);

        /// <summary>
        /// Runs a subset of processing focused on updating any cached beatmap object counts.
        /// </summary>
        /// <param name="beatmapInfo">The managed beatmap to update. A transaction will be opened to apply changes.</param>
        /// <param name="lookupScope">The preferred scope to use for metadata lookup.</param>
        void ProcessObjectCounts(BeatmapInfo beatmapInfo, MetadataLookupScope lookupScope = MetadataLookupScope.LocalCacheFirst);
    }
}
