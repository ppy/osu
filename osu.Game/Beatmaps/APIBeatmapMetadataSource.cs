// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Performs online metadata lookups using the osu-web API.
    /// </summary>
    public class APIBeatmapMetadataSource : IOnlineBeatmapMetadataSource
    {
        private readonly IAPIProvider api;

        public APIBeatmapMetadataSource(IAPIProvider api)
        {
            this.api = api;
        }

        public bool Available => api.State.Value == APIState.Online;

        public bool TryLookup(BeatmapInfo beatmapInfo, out OnlineBeatmapMetadata? onlineMetadata)
        {
            if (!Available)
            {
                onlineMetadata = null;
                return false;
            }

            Debug.Assert(beatmapInfo.BeatmapSet != null);

            var req = new GetBeatmapRequest(beatmapInfo);

            try
            {
                // intentionally blocking to limit web request concurrency
                api.Perform(req);

                if (req.CompletionState == APIRequestCompletionState.Failed)
                {
                    logForModel(beatmapInfo.BeatmapSet, $@"Online retrieval failed for {beatmapInfo}");
                    onlineMetadata = null;
                    return true;
                }

                var res = req.Response;

                if (res != null)
                {
                    logForModel(beatmapInfo.BeatmapSet, $@"Online retrieval mapped {beatmapInfo} to {res.OnlineBeatmapSetID} / {res.OnlineID}.");

                    onlineMetadata = new OnlineBeatmapMetadata
                    {
                        BeatmapID = res.OnlineID,
                        BeatmapSetID = res.OnlineBeatmapSetID,
                        AuthorID = res.AuthorID,
                        BeatmapStatus = res.Status,
                        BeatmapSetStatus = res.BeatmapSet?.Status,
                        DateRanked = res.BeatmapSet?.Ranked,
                        DateSubmitted = res.BeatmapSet?.Submitted,
                        MD5Hash = res.MD5Hash,
                        LastUpdated = res.LastUpdated
                    };
                    return true;
                }
            }
            catch (Exception e)
            {
                logForModel(beatmapInfo.BeatmapSet, $@"Online retrieval failed for {beatmapInfo} ({e.Message})");
                onlineMetadata = null;
                return false;
            }

            onlineMetadata = null;
            return false;
        }

        private void logForModel(BeatmapSetInfo set, string message) =>
            RealmArchiveModelImporter<BeatmapSetInfo>.LogForModel(set, $@"[{nameof(APIBeatmapMetadataSource)}] {message}");

        public void Dispose()
        {
        }
    }
}
