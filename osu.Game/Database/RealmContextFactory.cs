// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
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

        private int currentWriteUsages;

        private static readonly GlobalStatistic<int> reads = GlobalStatistics.Get<int>("Realm", "Get (Read)");
        private static readonly GlobalStatistic<int> writes = GlobalStatistics.Get<int>("Realm", "Get (Write)");
        private static readonly GlobalStatistic<int> refreshes = GlobalStatistics.Get<int>("Realm", "Dirty Refreshes");
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

            Interlocked.Increment(ref currentWriteUsages);
            return new RealmWriteUsage(this);
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

        private void onMigration(Migration migration, ulong lastSchemaVersion)
        {
        }

        public class RealmWriteUsage : InvokeOnDisposal<RealmContextFactory>
        {
            public readonly Realm Context;

            public RealmWriteUsage(RealmContextFactory factory)
                : base(factory, usageCompleted)
            {
                Context = factory.createContext();
                Context.BeginWrite();
            }

            private static void usageCompleted(RealmContextFactory factory)
            {
                Monitor.Exit(factory.writeLock);
            }
        }
    }
}
