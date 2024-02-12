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
                if (!tryLookup(beatmapInfo, preferOnlineFetch, out var res))
                    continue;

                if (res == null || shouldDiscardLookupResult(res, beatmapInfo))
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
                if (beatmapInfo.MatchesOnlineVersion)
                {
                    beatmapInfo.Status = res.BeatmapStatus;
                    beatmapInfo.Metadata.Author.OnlineID = res.AuthorID;
                }

                if (beatmapInfo.BeatmapSet.Beatmaps.All(b => b.MatchesOnlineVersion))
                {
                    beatmapInfo.BeatmapSet.Status = res.BeatmapSetStatus ?? BeatmapOnlineStatus.None;
                    beatmapInfo.BeatmapSet.DateRanked = res.DateRanked;
                    beatmapInfo.BeatmapSet.DateSubmitted = res.DateSubmitted;
                }
            }
        }

        private bool shouldDiscardLookupResult(OnlineBeatmapMetadata result, BeatmapInfo beatmapInfo)
        {
            if (beatmapInfo.OnlineID > 0 && result.BeatmapID != beatmapInfo.OnlineID)
                return true;

            if (beatmapInfo.OnlineID == -1 && result.MD5Hash != beatmapInfo.MD5Hash)
                return true;

            return false;
        }

        /// <summary>
        /// Attempts to retrieve the <see cref="OnlineBeatmapMetadata"/> for the given <paramref name="beatmapInfo"/>.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap to perform the online lookup for.</param>
        /// <param name="preferOnlineFetch">Whether online sources should be preferred for the lookup.</param>
        /// <param name="result">The result of the lookup. Can be <see langword="null"/> if no matching beatmap was found (or the lookup failed).</param>
        /// <returns>
        /// <see langword="true"/> if any of the metadata sources were available and returned a valid <paramref name="result"/>.
        /// <see langword="false"/> if none of the metadata sources were available, or if there was insufficient data to return a valid <paramref name="result"/>.
        /// </returns>
        /// <remarks>
        /// There are two cases wherein this method will return <see langword="false"/>:
        /// <list type="bullet">
        /// <item>If neither the local cache or the API are available to query.</item>
        /// <item>If the API is not available to query, and a positive match was not made in the local cache.</item>
        /// </list>
        /// In either case, the online ID read from the .osu file will be preserved, which may not necessarily be what we want.
        /// TODO: reconsider this if/when a better flow for queueing online retrieval is implemented.
        /// </remarks>
        private bool tryLookup(BeatmapInfo beatmapInfo, bool preferOnlineFetch, out OnlineBeatmapMetadata? result)
        {
            bool useLocalCache = !apiMetadataSource.Available || !preferOnlineFetch;
            if (useLocalCache && localCachedMetadataSource.TryLookup(beatmapInfo, out result))
                return true;

            if (apiMetadataSource.TryLookup(beatmapInfo, out result))
                return true;

            result = null;
            return false;
        }

        public void Dispose()
        {
            apiMetadataSource.Dispose();
            localCachedMetadataSource.Dispose();
        }
    }
}
