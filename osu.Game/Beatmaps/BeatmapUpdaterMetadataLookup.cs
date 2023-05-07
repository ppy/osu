// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Platform;
using osu.Game.Online.API;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A component which handles population of online IDs for beatmaps using a two part lookup procedure.
    /// </summary>
    public class BeatmapUpdaterMetadataLookup : IDisposable
    {
        private readonly IOnlineBeatmapMetadataSource apiMetadataSource;
        private readonly IOnlineBeatmapMetadataSource localCachedMetadataSource;

        public BeatmapUpdaterMetadataLookup(IAPIProvider api, Storage storage)
            : this(new APIBeatmapMetadataSource(api), new LocalCachedBeatmapMetadataSource(storage))
        {
        }

        internal BeatmapUpdaterMetadataLookup(IOnlineBeatmapMetadataSource apiMetadataSource, IOnlineBeatmapMetadataSource localCachedMetadataSource)
        {
            this.apiMetadataSource = apiMetadataSource;
            this.localCachedMetadataSource = localCachedMetadataSource;
        }

        /// <summary>
        /// Queue an update for a beatmap set.
        /// </summary>
        /// <remarks>
        /// This may happen during initial import, or at a later stage in response to a user action or server event.
        /// </remarks>
        /// <param name="beatmapSet">The beatmap set to update. Updates will be applied directly (so a transaction should be started if this instance is managed).</param>
        /// <param name="preferOnlineFetch">Whether metadata from an online source should be preferred. If <c>true</c>, the local cache will be skipped to ensure the freshest data state possible.</param>
        public void Update(BeatmapSetInfo beatmapSet, bool preferOnlineFetch)
        {
            foreach (var beatmapInfo in beatmapSet.Beatmaps)
            {
                var res = lookup(beatmapInfo, preferOnlineFetch);

                if (res == null)
                {
                    beatmapInfo.ResetOnlineInfo();
                    continue;
                }

                beatmapInfo.OnlineID = res.BeatmapID;
                beatmapInfo.OnlineMD5Hash = res.MD5Hash;
                beatmapInfo.LastOnlineUpdate = res.LastUpdated;

                Debug.Assert(beatmapInfo.BeatmapSet != null);
                beatmapInfo.BeatmapSet.OnlineID = res.BeatmapSetID;

                // Some metadata should only be applied if there's no local changes.
                if (shouldSaveOnlineMetadata(beatmapInfo))
                {
                    beatmapInfo.Status = res.BeatmapStatus;
                    beatmapInfo.Metadata.Author.OnlineID = res.AuthorID;
                }

                if (beatmapInfo.BeatmapSet.Beatmaps.All(shouldSaveOnlineMetadata))
                {
                    beatmapInfo.BeatmapSet.Status = res.BeatmapSetStatus ?? BeatmapOnlineStatus.None;
                    beatmapInfo.BeatmapSet.DateRanked = res.DateRanked;
                    beatmapInfo.BeatmapSet.DateSubmitted = res.DateSubmitted;
                }
            }
        }

        private OnlineBeatmapMetadata? lookup(BeatmapInfo beatmapInfo, bool preferOnlineFetch)
        {
            OnlineBeatmapMetadata? result = null;

            bool useLocalCache = !apiMetadataSource.Available || !preferOnlineFetch;

            if (useLocalCache)
                result = localCachedMetadataSource.Lookup(beatmapInfo);

            if (result != null)
                return result;

            if (apiMetadataSource.Available)
                result = apiMetadataSource.Lookup(beatmapInfo);

            return result;
        }

        /// <summary>
        /// Check whether the provided beatmap is in a state where online "ranked" status metadata should be saved against it.
        /// Handles the case where a user may have locally modified a beatmap in the editor and expects the local status to stick.
        /// </summary>
        private static bool shouldSaveOnlineMetadata(BeatmapInfo beatmapInfo) => beatmapInfo.MatchesOnlineVersion || beatmapInfo.Status != BeatmapOnlineStatus.LocallyModified;

        public void Dispose()
        {
            apiMetadataSource.Dispose();
            localCachedMetadataSource.Dispose();
        }
    }
}
