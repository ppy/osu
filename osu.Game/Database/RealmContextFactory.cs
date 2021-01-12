// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using Realms;

namespace osu.Game.Database
{
    public class RealmContextFactory : IRealmFactory
    {
        private readonly Storage storage;

        private const string database_name = @"client";

        private ThreadLocal<Realm> threadContexts;

        private readonly object writeLock = new object();

        private ThreadLocal<bool> refreshCompleted = new ThreadLocal<bool>();

        private bool rollbackRequired;

        private int currentWriteUsages;

        private Transaction currentWriteTransaction;

        public RealmContextFactory(Storage storage)
        {
            this.storage = storage;

            recreateThreadContexts();

            using (CreateContext())
            {
                // creating a context will ensure our schema is up-to-date and migrated.
            }
        }

        private void onMigration(Migration migration, ulong lastSchemaVersion)
        {
        }

        private static readonly GlobalStatistic<int> reads = GlobalStatistics.Get<int>("Realm", "Get (Read)");
        private static readonly GlobalStatistic<int> writes = GlobalStatistics.Get<int>("Realm", "Get (Write)");
        private static readonly GlobalStatistic<int> commits = GlobalStatistics.Get<int>("Realm", "Commits");
        private static readonly GlobalStatistic<int> rollbacks = GlobalStatistics.Get<int>("Realm", "Rollbacks");
        private static readonly GlobalStatistic<int> contexts = GlobalStatistics.Get<int>("Realm", "Contexts");

        /// <summary>
        /// Get a context for the current thread for read-only usage.
        /// If a <see cref="RealmWriteUsage"/> is in progress, the existing write-safe context will be returned.
        /// </summary>
        public Realm Get()
        {
            reads.Value++;
            return getContextForCurrentThread();
        }

        /// <summary>
        /// Request a context for write usage. Can be consumed in a nested fashion (and will return the same underlying context).
        /// This method may block if a write is already active on a different thread.
        /// </summary>
        /// <returns>A usage containing a usable context.</returns>
        public RealmWriteUsage GetForWrite()
        {
            writes.Value++;
            Monitor.Enter(writeLock);
            Realm context;

            try
            {
                context = getContextForCurrentThread();

                currentWriteTransaction ??= context.BeginWrite();
            }
            catch
            {
                // retrieval of a context could trigger a fatal error.
                Monitor.Exit(writeLock);
                throw;
            }

            Interlocked.Increment(ref currentWriteUsages);

            return new RealmWriteUsage(context, usageCompleted) { IsTransactionLeader = currentWriteTransaction != null && currentWriteUsages == 1 };
        }

        private Realm getContextForCurrentThread()
        {
            var context = threadContexts.Value;
            if (context?.IsClosed != false)
                threadContexts.Value = context = CreateContext();

            if (!refreshCompleted.Value)
            {
                context.Refresh();
                refreshCompleted.Value = true;
            }

            return context;
        }

        private void usageCompleted(RealmWriteUsage usage)
        {
            int usages = Interlocked.Decrement(ref currentWriteUsages);

            try
            {
                rollbackRequired |= usage.RollbackRequired;

                if (usages == 0)
                {
                    if (rollbackRequired)
                    {
                        rollbacks.Value++;
                        currentWriteTransaction?.Rollback();
                    }
                    else
                    {
                        commits.Value++;
                        currentWriteTransaction?.Commit();
                    }

                    currentWriteTransaction = null;
                    rollbackRequired = false;

                    refreshCompleted = new ThreadLocal<bool>();
                }
            }
            finally
            {
                Monitor.Exit(writeLock);
            }
        }

        private void recreateThreadContexts()
        {
            // Contexts for other threads are not disposed as they may be in use elsewhere. Instead, fresh contexts are exposed
            // for other threads to use, and we rely on the finalizer inside OsuDbContext to handle their previous contexts
            threadContexts?.Value.Dispose();
            threadContexts = new ThreadLocal<Realm>(CreateContext, true);
        }

        protected virtual Realm CreateContext()
        {
            contexts.Value++;
            return Realm.GetInstance(new RealmConfiguration(storage.GetFullPath($"{database_name}.realm", true))
            {
                SchemaVersion = 5,
                MigrationCallback = onMigration
            });
        }

        public void ResetDatabase()
        {
            lock (writeLock)
            {
                recreateThreadContexts();
                storage.DeleteDatabase(database_name);
            }
        }
    }
}
