// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Development;
using osu.Framework.Input.Bindings;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.Models;
using osu.Game.Skinning;
using osu.Game.Stores;
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

        private readonly IDatabaseContextFactory? efContextFactory;

        /// <summary>
        /// Version history:
        /// 6    ~2021-10-18   First tracked version.
        /// 7    2021-10-18    Changed OnlineID fields to non-nullable to add indexing support.
        /// 8    2021-10-29    Rebind scroll adjust keys to not have control modifier.
        /// 9    2021-11-04    Converted BeatmapMetadata.Author from string to RealmUser.
        /// 10   2021-11-22    Use ShortName instead of RulesetID for ruleset settings.
        /// 11   2021-11-22    Use ShortName instead of RulesetID for ruleset key bindings.
        /// 12   2021-11-24    Add Status to RealmBeatmapSet.
        /// </summary>
        private const int schema_version = 12;

        /// <summary>
        /// Lock object which is held during <see cref="BlockAllOperations"/> sections, blocking context creation during blocking periods.
        /// </summary>
        private readonly SemaphoreSlim contextCreationLock = new SemaphoreSlim(1);

        private readonly ThreadLocal<bool> currentThreadCanCreateContexts = new ThreadLocal<bool>();

        private static readonly GlobalStatistic<int> refreshes = GlobalStatistics.Get<int>(@"Realm", @"Dirty Refreshes");
        private static readonly GlobalStatistic<int> contexts_created = GlobalStatistics.Get<int>(@"Realm", @"Contexts (Created)");

        private readonly object contextLock = new object();
        private Realm? context;

        public Realm Context
        {
            get
            {
                if (!ThreadSafety.IsUpdateThread)
                    throw new InvalidOperationException(@$"Use {nameof(CreateContext)} when performing realm operations from a non-update thread");

                lock (contextLock)
                {
                    if (context == null)
                    {
                        context = CreateContext();
                        Logger.Log(@$"Opened realm ""{context.Config.DatabasePath}"" at version {context.Config.SchemaVersion}");
                    }

                    // creating a context will ensure our schema is up-to-date and migrated.
                    return context;
                }
            }
        }

        /// <summary>
        /// Construct a new instance of a realm context factory.
        /// </summary>
        /// <param name="storage">The game storage which will be used to create the realm backing file.</param>
        /// <param name="filename">The filename to use for the realm backing file. A ".realm" extension will be added automatically if not specified.</param>
        /// <param name="efContextFactory">An EF factory used only for migration purposes.</param>
        public RealmContextFactory(Storage storage, string filename, IDatabaseContextFactory? efContextFactory = null)
        {
            this.storage = storage;
            this.efContextFactory = efContextFactory;

            Filename = filename;

            const string realm_extension = @".realm";

            if (!Filename.EndsWith(realm_extension, StringComparison.Ordinal))
                Filename += realm_extension;

            // This method triggers the first `CreateContext` call, which will implicitly run realm migrations and bring the schema up-to-date.
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

                var pendingDeleteSkins = realm.All<SkinInfo>().Where(s => s.DeletePending);

                foreach (var s in pendingDeleteSkins)
                    realm.Remove(s);

                transaction.Commit();
            }

            // clean up files after dropping any pending deletions.
            // in the future we may want to only do this when the game is idle, rather than on every startup.
            new RealmFileStore(this, storage).Cleanup();
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

            bool tookSemaphoreLock = false;

            try
            {
                if (!currentThreadCanCreateContexts.Value)
                {
                    contextCreationLock.Wait();
                    currentThreadCanCreateContexts.Value = true;
                    tookSemaphoreLock = true;
                }
                else
                {
                    // the semaphore is used to handle blocking of all context creation during certain periods.
                    // once the semaphore has been taken by this code section, it is safe to create further contexts on the same thread.
                    // this can happen if a realm subscription is active and triggers a callback which has user code that calls `CreateContext`.
                }

                contexts_created.Value++;

                return Realm.GetInstance(getConfiguration());
            }
            finally
            {
                if (tookSemaphoreLock)
                {
                    contextCreationLock.Release();
                    currentThreadCanCreateContexts.Value = false;
                }
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
            for (ulong i = lastSchemaVersion + 1; i <= schema_version; i++)
                applyMigrationsForVersion(migration, i);
        }

        private void applyMigrationsForVersion(Migration migration, ulong targetVersion)
        {
            switch (targetVersion)
            {
                case 7:
                    convertOnlineIDs<RealmBeatmap>();
                    convertOnlineIDs<RealmBeatmapSet>();
                    convertOnlineIDs<RealmRuleset>();

                    void convertOnlineIDs<T>() where T : RealmObject
                    {
                        string className = getMappedOrOriginalName(typeof(T));

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

                    break;

                case 8:
                    // Ctrl -/+ now adjusts UI scale so let's clear any bindings which overlap these combinations.
                    // New defaults will be populated by the key store afterwards.
                    var keyBindings = migration.NewRealm.All<RealmKeyBinding>();

                    var increaseSpeedBinding = keyBindings.FirstOrDefault(k => k.ActionInt == (int)GlobalAction.IncreaseScrollSpeed);
                    if (increaseSpeedBinding != null && increaseSpeedBinding.KeyCombination.Keys.SequenceEqual(new[] { InputKey.Control, InputKey.Plus }))
                        migration.NewRealm.Remove(increaseSpeedBinding);

                    var decreaseSpeedBinding = keyBindings.FirstOrDefault(k => k.ActionInt == (int)GlobalAction.DecreaseScrollSpeed);
                    if (decreaseSpeedBinding != null && decreaseSpeedBinding.KeyCombination.Keys.SequenceEqual(new[] { InputKey.Control, InputKey.Minus }))
                        migration.NewRealm.Remove(decreaseSpeedBinding);

                    break;

                case 9:
                    // Pretty pointless to do this as beatmaps aren't really loaded via realm yet, but oh well.
                    string metadataClassName = getMappedOrOriginalName(typeof(RealmBeatmapMetadata));

                    // May be coming from a version before `RealmBeatmapMetadata` existed.
                    if (!migration.OldRealm.Schema.TryFindObjectSchema(metadataClassName, out _))
                        return;

                    var oldMetadata = migration.OldRealm.DynamicApi.All(metadataClassName);
                    var newMetadata = migration.NewRealm.All<RealmBeatmapMetadata>();

                    int metadataCount = newMetadata.Count();

                    for (int i = 0; i < metadataCount; i++)
                    {
                        dynamic? oldItem = oldMetadata.ElementAt(i);
                        var newItem = newMetadata.ElementAt(i);

                        string username = oldItem.Author;
                        newItem.Author = new RealmUser
                        {
                            Username = username
                        };
                    }

                    break;

                case 10:
                    string rulesetSettingClassName = getMappedOrOriginalName(typeof(RealmRulesetSetting));

                    if (!migration.OldRealm.Schema.TryFindObjectSchema(rulesetSettingClassName, out _))
                        return;

                    var oldSettings = migration.OldRealm.DynamicApi.All(rulesetSettingClassName);
                    var newSettings = migration.NewRealm.All<RealmRulesetSetting>().ToList();

                    for (int i = 0; i < newSettings.Count; i++)
                    {
                        dynamic? oldItem = oldSettings.ElementAt(i);
                        var newItem = newSettings.ElementAt(i);

                        long rulesetId = oldItem.RulesetID;
                        string? rulesetName = getRulesetShortNameFromLegacyID(rulesetId);

                        if (string.IsNullOrEmpty(rulesetName))
                            migration.NewRealm.Remove(newItem);
                        else
                            newItem.RulesetName = rulesetName;
                    }

                    break;

                case 11:
                    string keyBindingClassName = getMappedOrOriginalName(typeof(RealmKeyBinding));

                    if (!migration.OldRealm.Schema.TryFindObjectSchema(keyBindingClassName, out _))
                        return;

                    var oldKeyBindings = migration.OldRealm.DynamicApi.All(keyBindingClassName);
                    var newKeyBindings = migration.NewRealm.All<RealmKeyBinding>().ToList();

                    for (int i = 0; i < newKeyBindings.Count; i++)
                    {
                        dynamic? oldItem = oldKeyBindings.ElementAt(i);
                        var newItem = newKeyBindings.ElementAt(i);

                        if (oldItem.RulesetID == null)
                            continue;

                        long rulesetId = oldItem.RulesetID;
                        string? rulesetName = getRulesetShortNameFromLegacyID(rulesetId);

                        if (string.IsNullOrEmpty(rulesetName))
                            migration.NewRealm.Remove(newItem);
                        else
                            newItem.RulesetName = rulesetName;
                    }

                    break;
            }
        }

        private string? getRulesetShortNameFromLegacyID(long rulesetId) =>
            efContextFactory?.Get().RulesetInfo.FirstOrDefault(r => r.ID == rulesetId)?.ShortName;

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
                throw new InvalidOperationException(@$"{nameof(BlockAllOperations)} must be called from the update thread.");

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
                        throw new TimeoutException(@"Took too long to acquire lock");
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

        // https://github.com/realm/realm-dotnet/blob/32f4ebcc88b3e80a3b254412665340cd9f3bd6b5/Realm/Realm/Extensions/ReflectionExtensions.cs#L46
        private static string getMappedOrOriginalName(MemberInfo member) => member.GetCustomAttribute<MapToAttribute>()?.Mapping ?? member.Name;

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
