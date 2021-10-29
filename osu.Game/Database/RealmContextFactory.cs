// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Development;
using osu.Framework.Input.Bindings;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using osu.Game.Input.Bindings;
using osu.Game.Models;
using Realms;

#nullable enable

namespace osu.Game.Database
{
    /// <summary>
    /// A factory which provides both the main (update thread bound) realm context and creates contexts for async usage.
    /// </summary>
    public class RealmContextFactory : IDisposable, IRealmFactory
    {
        private readonly Storage storage;

        /// <summary>
        /// The filename of this realm.
        /// </summary>
        public readonly string Filename;

        /// <summary>
        /// Version history:
        /// 6  First tracked version (~20211018)
        /// 7  Changed OnlineID fields to non-nullable to add indexing support (20211018)
        /// 8  Rebind scroll adjust keys to not have control modifier (20211029)
        /// </summary>
        private const int schema_version = 8;

        /// <summary>
        /// Lock object which is held during <see cref="BlockAllOperations"/> sections, blocking context creation during blocking periods.
        /// </summary>
        private readonly SemaphoreSlim contextCreationLock = new SemaphoreSlim(1);

        private static readonly GlobalStatistic<int> refreshes = GlobalStatistics.Get<int>("Realm", "Dirty Refreshes");
        private static readonly GlobalStatistic<int> contexts_created = GlobalStatistics.Get<int>("Realm", "Contexts (Created)");

        private readonly object contextLock = new object();
        private Realm? context;

        public Realm Context
        {
            get
            {
                if (!ThreadSafety.IsUpdateThread)
                    throw new InvalidOperationException($"Use {nameof(CreateContext)} when performing realm operations from a non-update thread");

                lock (contextLock)
                {
                    if (context == null)
                    {
                        context = CreateContext();
                        Logger.Log($"Opened realm \"{context.Config.DatabasePath}\" at version {context.Config.SchemaVersion}");
                    }

                    // creating a context will ensure our schema is up-to-date and migrated.
                    return context;
                }
            }
        }

        public RealmContextFactory(Storage storage, string filename)
        {
            this.storage = storage;

            Filename = filename;

            const string realm_extension = ".realm";

            if (!Filename.EndsWith(realm_extension, StringComparison.Ordinal))
                Filename += realm_extension;

            cleanupPendingDeletions();
        }

        private void cleanupPendingDeletions()
        {
            using (var realm = CreateContext())
            using (var transaction = realm.BeginWrite())
            {
                var pendingDeleteSets = realm.All<RealmBeatmapSet>().Where(s => s.DeletePending);

                foreach (var s in pendingDeleteSets)
                {
                    foreach (var b in s.Beatmaps)
                        realm.Remove(b);

                    realm.Remove(s);
                }

                transaction.Commit();
            }
        }

        /// <summary>
        /// Compact this realm.
        /// </summary>
        /// <returns></returns>
        public bool Compact() => Realm.Compact(getConfiguration());

        /// <summary>
        /// Perform a blocking refresh on the main realm context.
        /// </summary>
        public void Refresh()
        {
            lock (contextLock)
            {
                if (context?.Refresh() == true)
                    refreshes.Value++;
            }
        }

        public Realm CreateContext()
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RealmContextFactory));

            try
            {
                contextCreationLock.Wait();

                contexts_created.Value++;

                return Realm.GetInstance(getConfiguration());
            }
            finally
            {
                contextCreationLock.Release();
            }
        }

        private RealmConfiguration getConfiguration()
        {
            return new RealmConfiguration(storage.GetFullPath(Filename, true))
            {
                SchemaVersion = schema_version,
                MigrationCallback = onMigration,
            };
        }

        private void onMigration(Migration migration, ulong lastSchemaVersion)
        {
            if (lastSchemaVersion < 8)
            {
                // Ctrl -/+ now adjusts UI scale so let's clear any bindings which overlap these combinations.
                // New defaults will be populated by the key store afterwards.
                var keyBindings = migration.NewRealm.All<RealmKeyBinding>();

                var increaseSpeedBinding = keyBindings.FirstOrDefault(k => k.ActionInt == (int)GlobalAction.IncreaseScrollSpeed);
                if (increaseSpeedBinding != null && increaseSpeedBinding.KeyCombination.Keys.SequenceEqual(new[] { InputKey.Control, InputKey.Plus }))
                    migration.NewRealm.Remove(increaseSpeedBinding);

                var decreaseSpeedBinding = keyBindings.FirstOrDefault(k => k.ActionInt == (int)GlobalAction.DecreaseScrollSpeed);
                if (decreaseSpeedBinding != null && decreaseSpeedBinding.KeyCombination.Keys.SequenceEqual(new[] { InputKey.Control, InputKey.Minus }))
                    migration.NewRealm.Remove(decreaseSpeedBinding);
            }

            if (lastSchemaVersion < 7)
            {
                convertOnlineIDs<RealmBeatmap>();
                convertOnlineIDs<RealmBeatmapSet>();
                convertOnlineIDs<RealmRuleset>();

                void convertOnlineIDs<T>() where T : RealmObject
                {
                    string className = typeof(T).Name.Replace(@"Realm", string.Empty);

                    // version was not bumped when the beatmap/ruleset models were added
                    // therefore we must manually check for their presence to avoid throwing on the `DynamicApi` calls.
                    if (!migration.OldRealm.Schema.TryFindObjectSchema(className, out _))
                        return;

                    var oldItems = migration.OldRealm.DynamicApi.All(className);
                    var newItems = migration.NewRealm.DynamicApi.All(className);

                    int itemCount = newItems.Count();

                    for (int i = 0; i < itemCount; i++)
                    {
                        dynamic? oldItem = oldItems.ElementAt(i);
                        dynamic? newItem = newItems.ElementAt(i);

                        long? nullableOnlineID = oldItem?.OnlineID;
                        newItem.OnlineID = (int)(nullableOnlineID ?? -1);
                    }
                }
            }
        }

        /// <summary>
        /// Flush any active contexts and block any further writes.
        /// </summary>
        /// <remarks>
        /// This should be used in places we need to ensure no ongoing reads/writes are occurring with realm.
        /// ie. to move the realm backing file to a new location.
        /// </remarks>
        /// <returns>An <see cref="IDisposable"/> which should be disposed to end the blocking section.</returns>
        public IDisposable BlockAllOperations()
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RealmContextFactory));

            if (!ThreadSafety.IsUpdateThread)
                throw new InvalidOperationException($"{nameof(BlockAllOperations)} must be called from the update thread.");

            Logger.Log(@"Blocking realm operations.", LoggingTarget.Database);

            try
            {
                contextCreationLock.Wait();

                lock (contextLock)
                {
                    context?.Dispose();
                    context = null;
                }

                const int sleep_length = 200;
                int timeout = 5000;

                // see https://github.com/realm/realm-dotnet/discussions/2657
                while (!Compact())
                {
                    Thread.Sleep(sleep_length);
                    timeout -= sleep_length;

                    if (timeout < 0)
                        throw new TimeoutException("Took too long to acquire lock");
                }
            }
            catch
            {
                contextCreationLock.Release();
                throw;
            }

            return new InvokeOnDisposal<RealmContextFactory>(this, factory =>
            {
                factory.contextCreationLock.Release();
                Logger.Log(@"Restoring realm operations.", LoggingTarget.Database);
            });
        }

        private bool isDisposed;

        public void Dispose()
        {
            lock (contextLock)
            {
                context?.Dispose();
            }

            if (!isDisposed)
            {
                // intentionally block context creation indefinitely. this ensures that nothing can start consuming a new context after disposal.
                contextCreationLock.Wait();
                contextCreationLock.Dispose();

                isDisposed = true;
            }
        }
    }
}
