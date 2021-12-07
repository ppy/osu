// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using osu.Framework.Development;
using osu.Framework.IO.Network;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A component which handles population of online IDs for beatmaps using a two part lookup procedure.
    /// </summary>
    /// <remarks>
    /// On creating the component, a copy of a database containing metadata for a large subset of beatmaps (stored to <see cref="cache_database_name"/>) will be downloaded if not already present locally.
    /// This will always be checked before doing a second online query to get required metadata.
    /// </remarks>
    [ExcludeFromDynamicCompile]
    public class BeatmapOnlineLookupQueue : IDisposable
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
            return Task.WhenAll(beatmapSet.Beatmaps.Select(b => UpdateAsync(beatmapSet, b, cancellationToken)).ToArray());
        }

        // todo: expose this when we need to do individual difficulty lookups.
        protected Task UpdateAsync(BeatmapSetInfo beatmapSet, BeatmapInfo beatmapInfo, CancellationToken cancellationToken)
            => Task.Factory.StartNew(() => lookup(beatmapSet, beatmapInfo), cancellationToken, TaskCreationOptions.HideScheduler | TaskCreationOptions.RunContinuationsAsynchronously, updateScheduler);

        private void lookup(BeatmapSetInfo set, BeatmapInfo beatmapInfo)
        {
            if (checkLocalCache(set, beatmapInfo))
                return;

            if (api?.State.Value != APIState.Online)
                return;

            var req = new GetBeatmapRequest(beatmapInfo);

            req.Failure += fail;

            try
            {
                // intentionally blocking to limit web request concurrency
                api.Perform(req);

                var res = req.Response;

                if (res != null)
                {
                    beatmapInfo.Status = res.Status;
                    beatmapInfo.BeatmapSet.Status = res.BeatmapSet?.Status ?? BeatmapOnlineStatus.None;
                    beatmapInfo.BeatmapSet.OnlineID = res.OnlineBeatmapSetID;
                    beatmapInfo.OnlineID = res.OnlineID;

                    if (beatmapInfo.Metadata != null)
                        beatmapInfo.Metadata.AuthorID = res.AuthorID;

                    if (beatmapInfo.BeatmapSet.Metadata != null)
                        beatmapInfo.BeatmapSet.Metadata.AuthorID = res.AuthorID;

                    logForModel(set, $"Online retrieval mapped {beatmapInfo} to {res.OnlineBeatmapSetID} / {res.OnlineID}.");
                }
            }
            catch (Exception e)
            {
                fail(e);
            }

            void fail(Exception e)
            {
                beatmapInfo.OnlineID = null;
                logForModel(set, $"Online retrieval failed for {beatmapInfo} ({e.Message})");
            }
        }

        private void prepareLocalCache()
        {
            string cacheFilePath = storage.GetFullPath(cache_database_name);
            string compressedCacheFilePath = $"{cacheFilePath}.bz2";

            cacheDownloadRequest = new FileWebRequest(compressedCacheFilePath, $"https://assets.ppy.sh/client-resources/{cache_database_name}.bz2?{DateTimeOffset.UtcNow:yyyyMMdd}");

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

        private bool checkLocalCache(BeatmapSetInfo set, BeatmapInfo beatmapInfo)
        {
            // download is in progress (or was, and failed).
            if (cacheDownloadRequest != null)
                return false;

            // database is unavailable.
            if (!storage.Exists(cache_database_name))
                return false;

            if (string.IsNullOrEmpty(beatmapInfo.MD5Hash)
                && string.IsNullOrEmpty(beatmapInfo.Path)
                && beatmapInfo.OnlineID == null)
                return false;

            try
            {
                using (var db = new SqliteConnection(DatabaseContextFactory.CreateDatabaseConnectionString("online.db", storage)))
                {
                    db.Open();

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText = "SELECT beatmapset_id, beatmap_id, approved, user_id FROM osu_beatmaps WHERE checksum = @MD5Hash OR beatmap_id = @OnlineID OR filename = @Path";

                        cmd.Parameters.Add(new SqliteParameter("@MD5Hash", beatmapInfo.MD5Hash));
                        cmd.Parameters.Add(new SqliteParameter("@OnlineID", beatmapInfo.OnlineID ?? (object)DBNull.Value));
                        cmd.Parameters.Add(new SqliteParameter("@Path", beatmapInfo.Path));

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var status = (BeatmapOnlineStatus)reader.GetByte(2);

                                beatmapInfo.Status = status;
                                beatmapInfo.BeatmapSet.Status = status;
                                beatmapInfo.BeatmapSet.OnlineID = reader.GetInt32(0);
                                beatmapInfo.OnlineID = reader.GetInt32(1);

                                if (beatmapInfo.Metadata != null)
                                    beatmapInfo.Metadata.AuthorID = reader.GetInt32(3);

                                if (beatmapInfo.BeatmapSet.Metadata != null)
                                    beatmapInfo.BeatmapSet.Metadata.AuthorID = reader.GetInt32(3);

                                logForModel(set, $"Cached local retrieval for {beatmapInfo}.");
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logForModel(set, $"Cached local retrieval for {beatmapInfo} failed with {ex}.");
            }

            return false;
        }

        private void logForModel(BeatmapSetInfo set, string message) =>
            ArchiveModelManager<BeatmapSetInfo, BeatmapSetFileInfo>.LogForModel(set, $"[{nameof(BeatmapOnlineLookupQueue)}] {message}");

        public void Dispose()
        {
            cacheDownloadRequest?.Dispose();
            updateScheduler?.Dispose();
        }
    }
}
