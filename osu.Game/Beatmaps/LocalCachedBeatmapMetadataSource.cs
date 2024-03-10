// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using osu.Framework.Development;
using osu.Framework.IO.Network;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Database;
using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;
using SQLitePCL;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Performs online metadata lookups using a copy of a database containing metadata for a large subset of beatmaps (stored to <see cref="cache_database_name"/>).
    /// The database will be asynchronously downloaded - if not already present locally - when this component is constructed.
    /// </summary>
    public class LocalCachedBeatmapMetadataSource : IOnlineBeatmapMetadataSource
    {
        private readonly Storage storage;

        private FileWebRequest? cacheDownloadRequest;

        private const string cache_database_name = @"online.db";

        public LocalCachedBeatmapMetadataSource(Storage storage)
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

            this.storage = storage;

            // avoid downloading / using cache for unit tests.
            if (!DebugUtils.IsNUnitRunning && !storage.Exists(cache_database_name))
                prepareLocalCache();
        }

        public bool Available =>
            // no download in progress.
            cacheDownloadRequest == null
            // cached database exists on disk.
            && storage.Exists(cache_database_name);

        public bool TryLookup(BeatmapInfo beatmapInfo, out OnlineBeatmapMetadata? onlineMetadata)
        {
            if (!Available)
            {
                onlineMetadata = null;
                return false;
            }

            if (string.IsNullOrEmpty(beatmapInfo.MD5Hash)
                && string.IsNullOrEmpty(beatmapInfo.Path)
                && beatmapInfo.OnlineID <= 0)
            {
                onlineMetadata = null;
                return false;
            }

            Debug.Assert(beatmapInfo.BeatmapSet != null);

            try
            {
                using (var db = new SqliteConnection(string.Concat(@"Data Source=", storage.GetFullPath(@"online.db", true))))
                {
                    db.Open();

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText =
                            @"SELECT beatmapset_id, beatmap_id, approved, user_id, checksum, last_update FROM osu_beatmaps WHERE checksum = @MD5Hash OR beatmap_id = @OnlineID OR filename = @Path";

                        cmd.Parameters.Add(new SqliteParameter(@"@MD5Hash", beatmapInfo.MD5Hash));
                        cmd.Parameters.Add(new SqliteParameter(@"@OnlineID", beatmapInfo.OnlineID));
                        cmd.Parameters.Add(new SqliteParameter(@"@Path", beatmapInfo.Path));

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                logForModel(beatmapInfo.BeatmapSet, $@"Cached local retrieval for {beatmapInfo}.");

                                onlineMetadata = new OnlineBeatmapMetadata
                                {
                                    BeatmapSetID = reader.GetInt32(0),
                                    BeatmapID = reader.GetInt32(1),
                                    BeatmapStatus = (BeatmapOnlineStatus)reader.GetByte(2),
                                    BeatmapSetStatus = (BeatmapOnlineStatus)reader.GetByte(2),
                                    AuthorID = reader.GetInt32(3),
                                    MD5Hash = reader.GetString(4),
                                    LastUpdated = reader.GetDateTimeOffset(5),
                                    // TODO: DateSubmitted and DateRanked are not provided by local cache.
                                };
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logForModel(beatmapInfo.BeatmapSet, $@"Cached local retrieval for {beatmapInfo} failed with {ex}.");
                onlineMetadata = null;
                return false;
            }

            onlineMetadata = null;
            return false;
        }

        private void prepareLocalCache()
        {
            string cacheFilePath = storage.GetFullPath(cache_database_name);
            string compressedCacheFilePath = $@"{cacheFilePath}.bz2";

            cacheDownloadRequest = new FileWebRequest(compressedCacheFilePath, $@"https://assets.ppy.sh/client-resources/{cache_database_name}.bz2?{DateTimeOffset.UtcNow:yyyyMMdd}");

            cacheDownloadRequest.Failed += ex =>
            {
                File.Delete(compressedCacheFilePath);
                File.Delete(cacheFilePath);

                Logger.Log($@"{nameof(BeatmapUpdaterMetadataLookup)}'s online cache download failed: {ex}", LoggingTarget.Database);
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
                    Logger.Log($@"{nameof(LocalCachedBeatmapMetadataSource)}'s online cache extraction failed: {ex}", LoggingTarget.Database);
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

        private void logForModel(BeatmapSetInfo set, string message) =>
            RealmArchiveModelImporter<BeatmapSetInfo>.LogForModel(set, $@"[{nameof(LocalCachedBeatmapMetadataSource)}] {message}");

        public void Dispose()
        {
            cacheDownloadRequest?.Dispose();
        }
    }
}
