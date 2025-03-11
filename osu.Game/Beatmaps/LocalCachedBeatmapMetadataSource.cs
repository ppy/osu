// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

            if (shouldFetchCache())
                prepareLocalCache();
        }

        private bool shouldFetchCache()
        {
            // avoid downloading / using cache for unit tests.
            if (DebugUtils.IsNUnitRunning)
                return false;

            if (!storage.Exists(cache_database_name))
            {
                log(@"Fetching local cache because it does not exist.");
                return true;
            }

            // periodically update the cache to include newer beatmaps.
            var fileInfo = new FileInfo(storage.GetFullPath(cache_database_name));

            if (fileInfo.LastWriteTime < DateTime.Now.AddMonths(-1))
            {
                log($@"Refetching local cache because it was last written to on {fileInfo.LastWriteTime}.");
                return true;
            }

            return false;
        }

        public bool Available =>
            // no download in progress.
            cacheDownloadRequest == null
            // cached database exists on disk.
            && storage.Exists(cache_database_name);

        public bool TryLookup(BeatmapInfo beatmapInfo, [NotNullWhen(true)] out OnlineBeatmapMetadata? onlineMetadata)
        {
            Debug.Assert(beatmapInfo.BeatmapSet != null);

            if (!Available)
            {
                onlineMetadata = null;
                return false;
            }

            if (string.IsNullOrEmpty(beatmapInfo.MD5Hash)
                && string.IsNullOrEmpty(beatmapInfo.Path))
            {
                onlineMetadata = null;
                return false;
            }

            try
            {
                using (var db = getConnection())
                {
                    db.Open();

                    switch (getCacheVersion(db))
                    {
                        case 2:
                            return queryCacheVersion2(db, beatmapInfo, out onlineMetadata);
                    }
                }

                onlineMetadata = null;
                return false;
            }
            catch (SqliteException sqliteException)
            {
                onlineMetadata = null;

                // There have been cases where the user's local database is corrupt.
                // Let's attempt to identify these cases and re-initialise the local cache.
                switch (sqliteException.SqliteErrorCode)
                {
                    case 26: // SQLITE_NOTADB
                    case 11: // SQLITE_CORRUPT
                        // only attempt purge & re-download if there is no other refetch in progress
                        if (cacheDownloadRequest != null)
                            return false;

                        tryPurgeCache();
                        prepareLocalCache();
                        return false;
                }

                logForModel(beatmapInfo.BeatmapSet, $@"Cached local retrieval for {beatmapInfo} failed with unhandled sqlite error {sqliteException}.");
                return false;
            }
            catch (Exception ex)
            {
                logForModel(beatmapInfo.BeatmapSet, $@"Cached local retrieval for {beatmapInfo} failed with {ex}.");
                onlineMetadata = null;
                return false;
            }
        }

        private void tryPurgeCache()
        {
            log(@"Local metadata cache is corrupted; attempting purge.");

            try
            {
                File.Delete(storage.GetFullPath(cache_database_name));
            }
            catch (Exception ex)
            {
                log($@"Failed to purge local metadata cache: {ex}");
            }

            log(@"Local metadata cache purged due to corruption.");
        }

        private SqliteConnection getConnection() =>
            new SqliteConnection(string.Concat(@"Data Source=", storage.GetFullPath(@"online.db", true)));

        private void prepareLocalCache()
        {
            bool isRefetch = storage.Exists(cache_database_name);

            string cacheFilePath = storage.GetFullPath(cache_database_name);
            string compressedCacheFilePath = $@"{cacheFilePath}.bz2";

            cacheDownloadRequest = new FileWebRequest(compressedCacheFilePath, $@"https://assets.ppy.sh/client-resources/{cache_database_name}.bz2?{DateTimeOffset.UtcNow:yyyyMMdd}");

            cacheDownloadRequest.Failed += ex =>
            {
                File.Delete(compressedCacheFilePath);

                // don't clobber the cache when refetching if the download didn't succeed. seems excessive.
                // consequently, also null the download request to allow the existing cache to be used (see `Available`).
                if (isRefetch)
                    cacheDownloadRequest = null;
                else
                    File.Delete(cacheFilePath);

                log($@"Online cache download failed: {ex}");
            };

            cacheDownloadRequest.Finished += () =>
            {
                try
                {
                    using (var stream = File.OpenRead(cacheDownloadRequest.Filename))
                    using (var outStream = File.OpenWrite(cacheFilePath))
                    {
                        // ensure to clobber any and all existing data to avoid accidental corruption.
                        outStream.SetLength(0);

                        using (var bz2 = new BZip2Stream(stream, CompressionMode.Decompress, false))
                            bz2.CopyTo(outStream);
                    }

                    // set to null on completion to allow lookups to begin using the new source
                    cacheDownloadRequest = null;
                    log(@"Local cache fetch completed successfully.");
                }
                catch (Exception ex)
                {
                    log($@"Online cache extraction failed: {ex}");
                    // at this point clobber the cache regardless of whether we're refetching, because by this point who knows what state the cache file is in.
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

        public int GetCacheVersion()
        {
            using (var connection = getConnection())
            {
                connection.Open();
                return getCacheVersion(connection);
            }
        }

        private int getCacheVersion(SqliteConnection connection)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT COUNT(1) FROM `sqlite_master` WHERE `type` = 'table' AND `name` = 'schema_version'";

                using var reader = cmd.ExecuteReader();

                if (!reader.Read())
                    throw new InvalidOperationException("Error when attempting to check for existence of `schema_version` table.");

                // No versioning table means that this is the very first version of the schema.
                if (reader.GetInt32(0) == 0)
                    return 1;
            }

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT `number` FROM `schema_version`";

                using var reader = cmd.ExecuteReader();

                if (!reader.Read())
                    throw new InvalidOperationException("Error when attempting to query schema version.");

                return reader.GetInt32(0);
            }
        }

        private bool queryCacheVersion2(SqliteConnection db, BeatmapInfo beatmapInfo, out OnlineBeatmapMetadata? onlineMetadata)
        {
            Debug.Assert(beatmapInfo.BeatmapSet != null);

            using var cmd = db.CreateCommand();

            cmd.CommandText =
                """
                SELECT `b`.`beatmapset_id`, `b`.`beatmap_id`, `b`.`approved`, `b`.`user_id`, `b`.`checksum`, `b`.`last_update`, `s`.`submit_date`, `s`.`approved_date`
                FROM `osu_beatmaps` AS `b`
                JOIN `osu_beatmapsets` AS `s` ON `s`.`beatmapset_id` = `b`.`beatmapset_id`
                WHERE (`b`.`checksum` = @MD5Hash OR `b`.`filename` = @Path)
                AND `b`.`approved` in (1, 2, 4)
                """;
            // approved conditional can theoretically be removed as it was fixed in
            // https://github.com/ppy/osu-onlinedb-generator/commit/489ac000775c3ff63bc914efb83cad0f6fbde261
            // but it's also safe to leave it (should not affect performance).

            cmd.Parameters.Add(new SqliteParameter(@"@MD5Hash", beatmapInfo.MD5Hash));
            cmd.Parameters.Add(new SqliteParameter(@"@Path", beatmapInfo.Path));

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                logForModel(beatmapInfo.BeatmapSet, $@"Cached local retrieval for {beatmapInfo} (cache version 2).");

                onlineMetadata = new OnlineBeatmapMetadata
                {
                    BeatmapSetID = reader.GetInt32(0),
                    BeatmapID = reader.GetInt32(1),
                    BeatmapStatus = (BeatmapOnlineStatus)reader.GetByte(2),
                    BeatmapSetStatus = (BeatmapOnlineStatus)reader.GetByte(2),
                    AuthorID = reader.GetInt32(3),
                    MD5Hash = reader.GetString(4),
                    LastUpdated = reader.GetDateTimeOffset(5),
                    DateSubmitted = reader.GetDateTimeOffset(6),
                    DateRanked = reader.GetDateTimeOffset(7),
                };
                return true;
            }

            onlineMetadata = null;
            return false;
        }

        private static void log(string message)
            => Logger.Log($@"[{nameof(LocalCachedBeatmapMetadataSource)}] {message}", LoggingTarget.Database);

        private void logForModel(BeatmapSetInfo set, string message) =>
            RealmArchiveModelImporter<BeatmapSetInfo>.LogForModel(set, $@"[{nameof(LocalCachedBeatmapMetadataSource)}] {message}");

        public void Dispose()
        {
            cacheDownloadRequest?.Dispose();
        }
    }
}
