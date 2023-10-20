// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using osu.Framework.Development;
using osu.Framework.IO.Network;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;
using SQLitePCL;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A component which handles population of online IDs for beatmaps using a two part lookup procedure.
    /// </summary>
    /// <remarks>
    /// On creating the component, a copy of a database containing metadata for a large subset of beatmaps (stored to <see cref="cache_database_name"/>) will be downloaded if not already present locally.
    /// This will always be checked before doing a second online query to get required metadata.
    /// </remarks>
    public class BeatmapUpdaterMetadataLookup : IDisposable
    {
        private readonly IAPIProvider api;
        private readonly Storage storage;

        private FileWebRequest cacheDownloadRequest;

        private const string cache_database_name = "online.db";

        public BeatmapUpdaterMetadataLookup(IAPIProvider api, Storage storage)
        {
            try
            {
                // required to initialise native SQLite libraries on some platforms.
                Batteries_V2.Init();
                raw.sqlite3_config(2 /*SQLITE_CONFIG_MULTITHREAD*/);
            }
            catch
            {
                // may fail if platform not supported.
            }

            this.api = api;
            this.storage = storage;

            // avoid downloading / using cache for unit tests.
            if (!DebugUtils.IsNUnitRunning && !storage.Exists(cache_database_name))
                prepareLocalCache();
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
            foreach (var b in beatmapSet.Beatmaps)
                lookup(beatmapSet, b, preferOnlineFetch);
        }

        private void lookup(BeatmapSetInfo set, BeatmapInfo beatmapInfo, bool preferOnlineFetch)
        {
            bool apiAvailable = api?.State.Value == APIState.Online;

            bool useLocalCache = !apiAvailable || !preferOnlineFetch;

            if (useLocalCache && checkLocalCache(set, beatmapInfo))
                return;

            if (!apiAvailable)
                return;

            var req = new GetBeatmapRequest(beatmapInfo);

            try
            {
                // intentionally blocking to limit web request concurrency
                api.Perform(req);

                if (req.CompletionState == APIRequestCompletionState.Failed)
                {
                    logForModel(set, $"Online retrieval failed for {beatmapInfo}");
                    beatmapInfo.ResetOnlineInfo();
                    return;
                }

                var res = req.Response;

                if (res != null)
                {
                    beatmapInfo.OnlineID = res.OnlineID;
                    beatmapInfo.OnlineMD5Hash = res.MD5Hash;
                    beatmapInfo.LastOnlineUpdate = res.LastUpdated;

                    Debug.Assert(beatmapInfo.BeatmapSet != null);
                    beatmapInfo.BeatmapSet.OnlineID = res.OnlineBeatmapSetID;

                    // Some metadata should only be applied if there's no local changes.
                    if (shouldSaveOnlineMetadata(beatmapInfo))
                    {
                        beatmapInfo.Status = res.Status;
                        beatmapInfo.Metadata.Author.OnlineID = res.AuthorID;
                    }

                    if (beatmapInfo.BeatmapSet.Beatmaps.All(shouldSaveOnlineMetadata))
                    {
                        beatmapInfo.BeatmapSet.Status = res.BeatmapSet?.Status ?? BeatmapOnlineStatus.None;
                        beatmapInfo.BeatmapSet.DateRanked = res.BeatmapSet?.Ranked;
                        beatmapInfo.BeatmapSet.DateSubmitted = res.BeatmapSet?.Submitted;
                    }

                    logForModel(set, $"Online retrieval mapped {beatmapInfo} to {res.OnlineBeatmapSetID} / {res.OnlineID}.");
                }
            }
            catch (Exception e)
            {
                logForModel(set, $"Online retrieval failed for {beatmapInfo} ({e.Message})");
                beatmapInfo.ResetOnlineInfo();
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

                Logger.Log($"{nameof(BeatmapUpdaterMetadataLookup)}'s online cache download failed: {ex}", LoggingTarget.Database);
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
                    Logger.Log($"{nameof(BeatmapUpdaterMetadataLookup)}'s online cache extraction failed: {ex}", LoggingTarget.Database);
                    File.Delete(cacheFilePath);
                }
                finally
                {
                    File.Delete(compressedCacheFilePath);
                }
            };

            Task.Run(async () =>
            {
                try
                {
                    await cacheDownloadRequest.PerformAsync().ConfigureAwait(false);
                }
                catch
                {
                    // Prevent throwing unobserved exceptions, as they will be logged from the network request to the log file anyway.
                }
            });
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
                && beatmapInfo.OnlineID <= 0)
                return false;

            try
            {
                using (var db = new SqliteConnection(string.Concat("Data Source=", storage.GetFullPath($@"{"online.db"}", true))))
                {
                    db.Open();

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText =
                            "SELECT beatmapset_id, beatmap_id, approved, user_id, checksum, last_update FROM osu_beatmaps WHERE checksum = @MD5Hash OR beatmap_id = @OnlineID OR filename = @Path";

                        cmd.Parameters.Add(new SqliteParameter("@MD5Hash", beatmapInfo.MD5Hash));
                        cmd.Parameters.Add(new SqliteParameter("@OnlineID", beatmapInfo.OnlineID));
                        cmd.Parameters.Add(new SqliteParameter("@Path", beatmapInfo.Path));

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var status = (BeatmapOnlineStatus)reader.GetByte(2);

                                // Some metadata should only be applied if there's no local changes.
                                if (shouldSaveOnlineMetadata(beatmapInfo))
                                {
                                    beatmapInfo.Status = status;
                                    beatmapInfo.Metadata.Author.OnlineID = reader.GetInt32(3);
                                }

                                // TODO: DateSubmitted and DateRanked are not provided by local cache.
                                beatmapInfo.OnlineID = reader.GetInt32(1);
                                beatmapInfo.OnlineMD5Hash = reader.GetString(4);
                                beatmapInfo.LastOnlineUpdate = reader.GetDateTimeOffset(5);

                                Debug.Assert(beatmapInfo.BeatmapSet != null);
                                beatmapInfo.BeatmapSet.OnlineID = reader.GetInt32(0);

                                if (beatmapInfo.BeatmapSet.Beatmaps.All(shouldSaveOnlineMetadata))
                                {
                                    beatmapInfo.BeatmapSet.Status = status;
                                }

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
            RealmArchiveModelImporter<BeatmapSetInfo>.LogForModel(set, $"[{nameof(BeatmapUpdaterMetadataLookup)}] {message}");

        /// <summary>
        /// Check whether the provided beatmap is in a state where online "ranked" status metadata should be saved against it.
        /// Handles the case where a user may have locally modified a beatmap in the editor and expects the local status to stick.
        /// </summary>
        private static bool shouldSaveOnlineMetadata(BeatmapInfo beatmapInfo) => beatmapInfo.MatchesOnlineVersion || beatmapInfo.Status != BeatmapOnlineStatus.LocallyModified;

        public void Dispose()
        {
            cacheDownloadRequest?.Dispose();
        }
    }
}
