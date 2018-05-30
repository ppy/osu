// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore.Storage;
using osu.Framework.Platform;

namespace osu.Game.Database
{
    public class DatabaseContextFactory : IDatabaseContextFactory
    {
        private readonly GameHost host;

        private const string database_name = @"client";

        private ThreadLocal<OsuDbContext> threadContexts;

        private readonly object writeLock = new object();

        private bool currentWriteDidWrite;
        private bool currentWriteDidError;

        private int currentWriteUsages;

        private IDbContextTransaction currentWriteTransaction;

        public DatabaseContextFactory(GameHost host)
        {
            this.host = host;
            recycleThreadContexts();
        }

        /// <summary>
        /// Get a context for the current thread for read-only usage.
        /// If a <see cref="DatabaseWriteUsage"/> is in progress, the existing write-safe context will be returned.
        /// </summary>
        public OsuDbContext Get() => threadContexts.Value;

        /// <summary>
        /// Request a context for write usage. Can be consumed in a nested fashion (and will return the same underlying context).
        /// This method may block if a write is already active on a different thread.
        /// </summary>
        /// <param name="withTransaction">Whether to start a transaction for this write.</param>
        /// <returns>A usage containing a usable context.</returns>
        public DatabaseWriteUsage GetForWrite(bool withTransaction = true)
        {
            Monitor.Enter(writeLock);

            if (currentWriteTransaction == null && withTransaction)
            {
                // this mitigates the fact that changes on tracked entities will not be rolled back with the transaction by ensuring write operations are always executed in isolated contexts.
                // if this results in sub-optimal efficiency, we may need to look into removing Database-level transactions in favour of running SaveChanges where we currently commit the transaction.
                if (threadContexts.IsValueCreated)
                    recycleThreadContexts();

                currentWriteTransaction = threadContexts.Value.Database.BeginTransaction();
            }

            Interlocked.Increment(ref currentWriteUsages);

            return new DatabaseWriteUsage(threadContexts.Value, usageCompleted) { IsTransactionLeader = currentWriteTransaction != null && currentWriteUsages == 1 };
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
                        currentWriteTransaction?.Rollback();
                    else
                        currentWriteTransaction?.Commit();

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

        private void recycleThreadContexts() => threadContexts = new ThreadLocal<OsuDbContext>(CreateContext);

        protected virtual OsuDbContext CreateContext()
        {
            var ctx = new OsuDbContext(host.Storage.GetDatabaseConnectionString(database_name));
            ctx.Database.AutoTransactionsEnabled = false;

            return ctx;
        }

        public void ResetDatabase()
        {
            lock (writeLock)
            {
                recycleThreadContexts();
                host.Storage.DeleteDatabase(database_name);
            }
        }
    }
}
