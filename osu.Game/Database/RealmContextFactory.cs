// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using osu.Game.Input.Bindings;
using Realms;

namespace osu.Game.Database
{
    public class RealmContextFactory : Component, IRealmFactory
    {
        private readonly Storage storage;

        private const string database_name = @"client";

        private const int schema_version = 6;

        /// <summary>
        /// Lock object which is held for the duration of a write operation (via <see cref="GetForWrite"/>).
        /// </summary>
        private readonly object writeLock = new object();

        private static readonly GlobalStatistic<int> reads = GlobalStatistics.Get<int>("Realm", "Get (Read)");
        private static readonly GlobalStatistic<int> writes = GlobalStatistics.Get<int>("Realm", "Get (Write)");
        private static readonly GlobalStatistic<int> refreshes = GlobalStatistics.Get<int>("Realm", "Dirty Refreshes");
        private static readonly GlobalStatistic<int> contexts_created = GlobalStatistics.Get<int>("Realm", "Contexts (Created)");
        private static readonly GlobalStatistic<int> pending_writes = GlobalStatistics.Get<int>("Realm", "Pending writes");
        private static readonly GlobalStatistic<int> active_usages = GlobalStatistics.Get<int>("Realm", "Active usages");

        private readonly ManualResetEventSlim blockingResetEvent = new ManualResetEventSlim(true);

        private Realm context;

        public Realm Context
        {
            get
            {
                if (IsDisposed)
                    throw new InvalidOperationException($"Attempted to access {nameof(Context)} on a disposed context factory");

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

        public RealmUsage GetForRead()
        {
            reads.Value++;
            return new RealmUsage(this);
        }

        public RealmWriteUsage GetForWrite()
        {
            writes.Value++;
            pending_writes.Value++;

            Monitor.Enter(writeLock);

            return new RealmWriteUsage(this);
        }

        protected override void Update()
        {
            base.Update();

            if (context?.Refresh() == true)
                refreshes.Value++;
        }

        private Realm createContext()
        {
            blockingResetEvent.Wait();

            contexts_created.Value++;

            return Realm.GetInstance(new RealmConfiguration(storage.GetFullPath($"{database_name}.realm", true))
            {
                SchemaVersion = schema_version,
                MigrationCallback = onMigration,
            });
        }

        private void onMigration(Migration migration, ulong lastSchemaVersion)
        {
            switch (lastSchemaVersion)
            {
                case 5:
                    // let's keep things simple. changing the type of the primary key is a bit involved.
                    migration.NewRealm.RemoveAll<RealmKeyBinding>();
                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            BlockAllOperations();
        }

        public IDisposable BlockAllOperations()
        {
            blockingResetEvent.Reset();
            flushContexts();

            return new InvokeOnDisposal<RealmContextFactory>(this, r => endBlockingSection());
        }

        private void endBlockingSection()
        {
            blockingResetEvent.Set();
        }

        private void flushContexts()
        {
            var previousContext = context;
            context = null;

            // wait for all threaded usages to finish
            while (active_usages.Value > 0)
                Thread.Sleep(50);

            previousContext?.Dispose();
        }

        /// <summary>
        /// A usage of realm from an arbitrary thread.
        /// </summary>
        public class RealmUsage : IDisposable
        {
            public readonly Realm Realm;

            protected readonly RealmContextFactory Factory;

            internal RealmUsage(RealmContextFactory factory)
            {
                active_usages.Value++;
                Factory = factory;
                Realm = factory.createContext();
            }

            /// <summary>
            /// Disposes this instance, calling the initially captured action.
            /// </summary>
            public virtual void Dispose()
            {
                Realm?.Dispose();
                active_usages.Value--;
            }
        }

        /// <summary>
        /// A transaction used for making changes to realm data.
        /// </summary>
        public class RealmWriteUsage : RealmUsage
        {
            private readonly Transaction transaction;

            internal RealmWriteUsage(RealmContextFactory factory)
                : base(factory)
            {
                transaction = Realm.BeginWrite();
            }

            /// <summary>
            /// Commit all changes made in this transaction.
            /// </summary>
            public void Commit() => transaction.Commit();

            /// <summary>
            /// Revert all changes made in this transaction.
            /// </summary>
            public void Rollback() => transaction.Rollback();

            /// <summary>
            /// Disposes this instance, calling the initially captured action.
            /// </summary>
            public override void Dispose()
            {
                // rollback if not explicitly committed.
                transaction?.Dispose();

                base.Dispose();

                Monitor.Exit(Factory.writeLock);
                pending_writes.Value--;
            }
        }
    }
}
