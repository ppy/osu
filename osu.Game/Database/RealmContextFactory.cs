// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using Realms;

namespace osu.Game.Database
{
    public class RealmContextFactory : IRealmFactory
    {
        private readonly Storage storage;

        private const string database_name = @"client";

        private const int schema_version = 5;

        private readonly ThreadLocal<Realm> threadContexts;

        /// <summary>
        /// Lock object which is held for the duration of a write operation (via <see cref="GetForWrite"/>).
        /// </summary>
        private readonly object writeLock = new object();

        private ThreadLocal<bool> refreshCompleted = new ThreadLocal<bool>();

        private bool rollbackRequired;

        private int currentWriteUsages;

        private Transaction currentWriteTransaction;

        public RealmContextFactory(Storage storage)
        {
            this.storage = storage;

            threadContexts = new ThreadLocal<Realm>(createContext, true);

            // creating a context will ensure our schema is up-to-date and migrated.
            var realm = Get();
            Logger.Log($"Opened realm \"{realm.Config.DatabasePath}\" at version {realm.Config.SchemaVersion}");
        }

        private void onMigration(Migration migration, ulong lastSchemaVersion)
        {
        }

        private static readonly GlobalStatistic<int> reads = GlobalStatistics.Get<int>("Realm", "Get (Read)");
        private static readonly GlobalStatistic<int> writes = GlobalStatistics.Get<int>("Realm", "Get (Write)");
        private static readonly GlobalStatistic<int> commits = GlobalStatistics.Get<int>("Realm", "Commits");
        private static readonly GlobalStatistic<int> rollbacks = GlobalStatistics.Get<int>("Realm", "Rollbacks");
        private static readonly GlobalStatistic<int> contexts_open = GlobalStatistics.Get<int>("Realm", "Contexts (Open)");
        private static readonly GlobalStatistic<int> contexts_created = GlobalStatistics.Get<int>("Realm", "Contexts (Created)");

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
                threadContexts.Value = context = createContext();

            contexts_open.Value = threadContexts.Values.Count;

            if (!refreshCompleted.Value)
            {
                // to keep things simple, realm refreshes are currently performed per thread context at the point of retrieval.
                // in the future this should likely be run as part of the update loop for the main (update thread) context.
                context.Refresh();
                refreshCompleted.Value = true;
            }

            return context;
        }

        private Realm createContext()
        {
            contexts_created.Value++;

            return Realm.GetInstance(new RealmConfiguration(storage.GetFullPath($"{database_name}.realm", true))
            {
                SchemaVersion = schema_version,
                MigrationCallback = onMigration,
            });
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
    }
}
