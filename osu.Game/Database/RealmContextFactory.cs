// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Development;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Statistics;
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

        private const int schema_version = 6;

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
