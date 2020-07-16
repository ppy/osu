// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using osu.Framework.Development;
using osu.Framework.IO.Network;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;

namespace osu.Game.Beatmaps
{
    public partial class BeatmapManager
    {
        private class BeatmapOnlineLookupQueue : IDisposable
        {
            private readonly IAPIProvider api;
            private readonly Storage storage;

            private const int update_queue_request_concurrency = 4;

            private readonly ThreadedTaskScheduler updateScheduler = new ThreadedTaskScheduler(update_queue_request_concurrency, nameof(BeatmapOnlineLookupQueue));

            private FileWebRequest cacheDownloadRequest;

            private const string cache_database_name = "online.db";

            public BeatmapOnlineLookupQueue(IAPIProvider api, Storage storage)
            {
                this.api = api;
                this.storage = storage;

                // avoid downloading / using cache for unit tests.
                if (!DebugUtils.IsNUnitRunning && !storage.Exists(cache_database_name))
                    prepareLocalCache();
            }

            public Task UpdateAsync(BeatmapSetInfo beatmapSet, CancellationToken cancellationToken)
            {
                LogForModel(beatmapSet, "Performing online lookups...");
                return Task.WhenAll(beatmapSet.Beatmaps.Select(b => UpdateAsync(beatmapSet, b, cancellationToken)).ToArray());
            }

            // todo: expose this when we need to do individual difficulty lookups.
            protected Task UpdateAsync(BeatmapSetInfo beatmapSet, BeatmapInfo beatmap, CancellationToken cancellationToken)
                => Task.Factory.StartNew(() => lookup(beatmapSet, beatmap), cancellationToken, TaskCreationOptions.HideScheduler | TaskCreationOptions.RunContinuationsAsynchronously, updateScheduler);

            private void lookup(BeatmapSetInfo set, BeatmapInfo beatmap)
            {
                if (checkLocalCache(set, beatmap))
                    return;

                if (api?.State != APIState.Online)
                    return;

                var req = new GetBeatmapRequest(beatmap);

                req.Failure += fail;

                try
                {
                    // intentionally blocking to limit web request concurrency
                    api.Perform(req);

                    var res = req.Result;

                    if (res != null)
                    {
                        beatmap.Status = res.Status;
                        beatmap.BeatmapSet.Status = res.BeatmapSet.Status;
                        beatmap.BeatmapSet.OnlineBeatmapSetID = res.OnlineBeatmapSetID;
                        beatmap.OnlineBeatmapID = res.OnlineBeatmapID;

                        LogForModel(set, $"Online retrieval mapped {beatmap} to {res.OnlineBeatmapSetID} / {res.OnlineBeatmapID}.");
                    }
                }
                catch (Exception e)
                {
                    fail(e);
                }

                void fail(Exception e)
                {
                    beatmap.OnlineBeatmapID = null;
                    LogForModel(set, $"Online retrieval failed for {beatmap} ({e.Message})");
                }
            }

            private void prepareLocalCache()
            {
                string cacheFilePath = storage.GetFullPath(cache_database_name);
                string compressedCacheFilePath = $"{cacheFilePath}.bz2";

                cacheDownloadRequest = new FileWebRequest(compressedCacheFilePath, $"https://assets.ppy.sh/client-resources/{cache_database_name}.bz2");

                cacheDownloadRequest.Failed += ex =>
                {
                    File.Delete(compressedCacheFilePath);
                    File.Delete(cacheFilePath);

                    Logger.Log($"{nameof(BeatmapOnlineLookupQueue)}'s online cache download failed: {ex}", LoggingTarget.Database);
                };

                cacheDownloadRequest.Finished += () =>
                {
                    try
                    {
                        using (var stream = File.OpenRead(cacheDownloadRequest.Filename))
                        using (var outStream = File.OpenWrite(cacheFilePath))
                        using (var bz2 = new BZip2Stream(stream, CompressionMode.Decompress, false))
                            bz2.CopyTo(outStream);

                        // set to null on completion to allow lookups to begin using the new source
                        cacheDownloadRequest = null;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"{nameof(BeatmapOnlineLookupQueue)}'s online cache extraction failed: {ex}", LoggingTarget.Database);
                        File.Delete(cacheFilePath);
                    }
                    finally
                    {
                        File.Delete(compressedCacheFilePath);
                    }
                };

                cacheDownloadRequest.PerformAsync();
            }

            private bool checkLocalCache(BeatmapSetInfo set, BeatmapInfo beatmap)
            {
                // download is in progress (or was, and failed).
                if (cacheDownloadRequest != null)
                    return false;

                // database is unavailable.
                if (!storage.Exists(cache_database_name))
                    return false;

                try
                {
                    using (var db = new SqliteConnection(storage.GetDatabaseConnectionString("online")))
                    {
                        var found = db.QuerySingleOrDefault<CachedOnlineBeatmapLookup>(
                            "SELECT * FROM osu_beatmaps WHERE checksum = @MD5Hash OR beatmap_id = @OnlineBeatmapID OR filename = @Path", beatmap);

                        if (found != null)
                        {
                            var status = (BeatmapSetOnlineStatus)found.approved;

                            beatmap.Status = status;
                            beatmap.BeatmapSet.Status = status;
                            beatmap.BeatmapSet.OnlineBeatmapSetID = found.beatmapset_id;
                            beatmap.OnlineBeatmapID = found.beatmap_id;

                            LogForModel(set, $"Cached local retrieval for {beatmap}.");
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogForModel(set, $"Cached local retrieval for {beatmap} failed with {ex}.");
                }

                return false;
            }

            public void Dispose()
            {
                cacheDownloadRequest?.Dispose();
                updateScheduler?.Dispose();
            }

            [Serializable]
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            private class CachedOnlineBeatmapLookup
            {
                public int approved { get; set; }

                public int? beatmapset_id { get; set; }

                public int? beatmap_id { get; set; }
            }
        }
    }
}
