// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore.Storage;
using osu.Framework.Development;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Statistics;

namespace osu.Game.Database
{
    public class DatabaseContextFactory : IDatabaseContextFactory
    {
        private readonly Storage storage;

        public const string DATABASE_NAME = @"client.db";

        private ThreadLocal<OsuDbContext> threadContexts;

        private readonly object writeLock = new object();

        private bool currentWriteDidWrite;
        private bool currentWriteDidError;

        private int currentWriteUsages;

        private IDbContextTransaction currentWriteTransaction;

        public DatabaseContextFactory(Storage storage)
        {
            this.storage = storage;
            recycleThreadContexts();
        }

        private static readonly GlobalStatistic<int> reads = GlobalStatistics.Get<int>("Database", "Get (Read)");
        private static readonly GlobalStatistic<int> writes = GlobalStatistics.Get<int>("Database", "Get (Write)");
        private static readonly GlobalStatistic<int> commits = GlobalStatistics.Get<int>("Database", "Commits");
        private static readonly GlobalStatistic<int> rollbacks = GlobalStatistics.Get<int>("Database", "Rollbacks");

        /// <summary>
        /// Get a context for the current thread for read-only usage.
        /// If a <see cref="DatabaseWriteUsage"/> is in progress, the existing write-safe context will be returned.
        /// </summary>
        public OsuDbContext Get()
        {
            reads.Value++;
            return threadContexts.Value;
        }

        /// <summary>
        /// Request a context for write usage. Can be consumed in a nested fashion (and will return the same underlying context).
        /// This method may block if a write is already active on a different thread.
        /// </summary>
        /// <param name="withTransaction">Whether to start a transaction for this write.</param>
        /// <returns>A usage containing a usable context.</returns>
        public DatabaseWriteUsage GetForWrite(bool withTransaction = true)
        {
            writes.Value++;
            Monitor.Enter(writeLock);
            OsuDbContext context;

            try
            {
                if (currentWriteTransaction == null && withTransaction)
                {
                    // this mitigates the fact that changes on tracked entities will not be rolled back with the transaction by ensuring write operations are always executed in isolated contexts.
                    // if this results in sub-optimal efficiency, we may need to look into removing Database-level transactions in favour of running SaveChanges where we currently commit the transaction.
                    if (threadContexts.IsValueCreated)
                        recycleThreadContexts();

                    context = threadContexts.Value;
                    currentWriteTransaction = context.Database.BeginTransaction();
                }
                else
                {
                    // we want to try-catch the retrieval of the context because it could throw an error (in CreateContext).
                    context = threadContexts.Value;
                }
            }
            catch
            {
                // retrieval of a context could trigger a fatal error.
                Monitor.Exit(writeLock);
                throw;
            }

            Interlocked.Increment(ref currentWriteUsages);

            return new DatabaseWriteUsage(context, usageCompleted) { IsTransactionLeader = currentWriteTransaction != null && currentWriteUsages == 1 };
        }

        private void usageCompleted(DatabaseWriteUsage usage)
        {
            int usages = Interlocked.Decrement(ref currentWriteUsages);

            try
            {
                currentWriteDidWrite |= usage.PerformedWrite;
                currentWriteDidError |= usage.Errors.Any();

                if (usages == 0)
                {
                    if (currentWriteDidError)
                    {
                        rollbacks.Value++;
                        currentWriteTransaction?.Rollback();
                    }
                    else
                    {
                        commits.Value++;
                        currentWriteTransaction?.Commit();
                    }

                    if (currentWriteDidWrite || currentWriteDidError)
                    {
                        // explicitly dispose to ensure any outstanding flushes happen as soon as possible (and underlying resources are purged).
                        usage.Context.Dispose();

                        // once all writes are complete, we want to refresh thread-specific contexts to make sure they don't have stale local caches.
                        recycleThreadContexts();
                    }

                    currentWriteTransaction = null;
                    currentWriteDidWrite = false;
                    currentWriteDidError = false;
                }
            }
            finally
            {
                Monitor.Exit(writeLock);
            }
        }

        private void recycleThreadContexts()
        {
            // Contexts for other threads are not disposed as they may be in use elsewhere. Instead, fresh contexts are exposed
            // for other threads to use, and we rely on the finalizer inside OsuDbContext to handle their previous contexts
            threadContexts?.Value.Dispose();
            threadContexts = new ThreadLocal<OsuDbContext>(CreateContext, true);
        }

        protected virtual OsuDbContext CreateContext() => new OsuDbContext(CreateDatabaseConnectionString(DATABASE_NAME, storage))
        {
            Database = { AutoTransactionsEnabled = false }
        };

        public void CreateBackup(string backupFilename)
        {
            Logger.Log($"Creating full EF database backup at {backupFilename}", LoggingTarget.Database);

            if (DebugUtils.IsDebugBuild)
                Logger.Log("Your development database has been fully migrated to realm. If you switch back to a pre-realm branch and need your previous database, rename the backup file back to \"client.db\".\n\nNote that doing this can potentially leave your file store in a bad state.", level: LogLevel.Important);

            using (var source = storage.GetStream(DATABASE_NAME))
            using (var destination = storage.GetStream(backupFilename, FileAccess.Write, FileMode.CreateNew))
                source.CopyTo(destination);
        }

        public void ResetDatabase()
        {
            lock (writeLock)
            {
                recycleThreadContexts();

                try
                {
                    storage.Delete(DATABASE_NAME);
                }
                catch
                {
                    // for now we are not sure why file handles are kept open by EF, but this is generally only used in testing
                }
            }
        }

        public void FlushConnections()
        {
            if (threadContexts != null)
            {
                foreach (var context in threadContexts.Values)
                    context.Dispose();
            }

            recycleThreadContexts();
        }

        public static string CreateDatabaseConnectionString(string filename, Storage storage) => string.Concat("Data Source=", storage.GetFullPath($@"{filename}", true));
    }
}
