// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using Realms;

namespace osu.Game.Database
{
    public class RealmContextFactory : Component, IRealmFactory
    {
        private readonly Storage storage;
        private const string database_name = @"client";

        private const int schema_version = 5;

        /// <summary>
        /// Lock object which is held for the duration of a write operation (via <see cref="GetForWrite"/>).
        /// </summary>
        private readonly object writeLock = new object();

        private ThreadLocal<bool> refreshCompleted = new ThreadLocal<bool>();

        private bool rollbackRequired;

        private int currentWriteUsages;

        private Transaction currentWriteTransaction;

        private static readonly GlobalStatistic<int> reads = GlobalStatistics.Get<int>("Realm", "Get (Read)");
        private static readonly GlobalStatistic<int> writes = GlobalStatistics.Get<int>("Realm", "Get (Write)");
        private static readonly GlobalStatistic<int> refreshes = GlobalStatistics.Get<int>("Realm", "Refreshes");
        private static readonly GlobalStatistic<int> commits = GlobalStatistics.Get<int>("Realm", "Commits");
        private static readonly GlobalStatistic<int> rollbacks = GlobalStatistics.Get<int>("Realm", "Rollbacks");
        private static readonly GlobalStatistic<int> contexts_open = GlobalStatistics.Get<int>("Realm", "Contexts (Open)");
        private static readonly GlobalStatistic<int> contexts_created = GlobalStatistics.Get<int>("Realm", "Contexts (Created)");

        private Realm context;

        public Realm Context
        {
            get
            {
                if (context == null)
                {
                    context = createContext();
                    Logger.Log($"Opened realm \"{context.Config.DatabasePath}\" at version {context.Config.SchemaVersion}");
                }

                // creating a context will ensure our schema is up-to-date and migrated.

                return context;
            }
        }

        public RealmContextFactory(Storage storage)
        {
            this.storage = storage;
        }

        public Realm GetForRead()
        {
            reads.Value++;
            return createContext();
        }

        public RealmWriteUsage GetForWrite()
        {
            writes.Value++;
            Monitor.Enter(writeLock);

            Realm realm;

            try
            {
                realm = createContext();

                currentWriteTransaction ??= realm.BeginWrite();
            }
            catch
            {
                // retrieval of a context could trigger a fatal error.
                Monitor.Exit(writeLock);
                throw;
            }

            Interlocked.Increment(ref currentWriteUsages);

            return new RealmWriteUsage(realm, usageCompleted) { IsTransactionLeader = currentWriteTransaction != null && currentWriteUsages == 1 };
        }

        protected override void Update()
        {
            base.Update();

            if (Context.Refresh())
                refreshes.Value++;
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

        private void onMigration(Migration migration, ulong lastSchemaVersion)
        {
        }
    }
}
