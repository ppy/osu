// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.Models;
using osu.Game.Skinning;
using osu.Game.Stores;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using Realms;
using Realms.Exceptions;

#nullable enable

namespace osu.Game.Database
{
    /// <summary>
    /// A factory which provides both the main (update thread bound) realm context and creates contexts for async usage.
    /// </summary>
    public class RealmContextFactory : IDisposable
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
        /// 13   2022-01-13    Final migration of beatmaps and scores to realm (multiple new storage fields).
        /// </summary>
        private const int schema_version = 13;

        /// <summary>
        /// Lock object which is held during <see cref="BlockAllOperations"/> sections, blocking context creation during blocking periods.
        /// </summary>
        private readonly SemaphoreSlim contextCreationLock = new SemaphoreSlim(1);

        private readonly ThreadLocal<bool> currentThreadCanCreateContexts = new ThreadLocal<bool>();

        private static readonly GlobalStatistic<int> contexts_created = GlobalStatistics.Get<int>(@"Realm", @"Contexts (Created)");

        private readonly object contextLock = new object();

        private Realm? context;

        public Realm Context => ensureUpdateContext();

        private Realm ensureUpdateContext()
        {
            if (!ThreadSafety.IsUpdateThread)
                throw new InvalidOperationException(@$"Use {nameof(createContext)} when performing realm operations from a non-update thread");

            lock (contextLock)
            {
                if (context == null)
                {
                    context = createContext();
                    Logger.Log(@$"Opened realm ""{context.Config.DatabasePath}"" at version {context.Config.SchemaVersion}");

                    // Resubscribe any subscriptions
                    foreach (var action in customSubscriptionActions.Keys)
                        registerSubscription(action);
                }

                Debug.Assert(context != null);

                // creating a context will ensure our schema is up-to-date and migrated.
                return context;
            }
        }

        internal static bool CurrentThreadSubscriptionsAllowed => current_thread_subscriptions_allowed.Value;

        private static readonly ThreadLocal<bool> current_thread_subscriptions_allowed = new ThreadLocal<bool>();

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

            try
            {
                // This method triggers the first `CreateContext` call, which will implicitly run realm migrations and bring the schema up-to-date.
                cleanupPendingDeletions();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Realm startup failed with unrecoverable error; starting with a fresh database. A backup of your database has been made.");

                CreateBackup($"{Filename.Replace(realm_extension, string.Empty)}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_corrupt{realm_extension}");
                storage.Delete(Filename);

                cleanupPendingDeletions();
            }
        }

        private void cleanupPendingDeletions()
        {
            using (var realm = createContext())
            using (var transaction = realm.BeginWrite())
            {
                var pendingDeleteScores = realm.All<ScoreInfo>().Where(s => s.DeletePending);

                foreach (var score in pendingDeleteScores)
                    realm.Remove(score);

                var pendingDeleteSets = realm.All<BeatmapSetInfo>().Where(s => s.DeletePending);

                foreach (var beatmapSet in pendingDeleteSets)
                {
                    foreach (var beatmap in beatmapSet.Beatmaps)
                    {
                        // Cascade delete related scores, else they will have a null beatmap against the model's spec.
                        foreach (var score in beatmap.Scores)
                            realm.Remove(score);

                        realm.Remove(beatmap.Metadata);

                        realm.Remove(beatmap);
                    }

                    realm.Remove(beatmapSet);
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
        /// Run work on realm with a return value.
        /// </summary>
        /// <remarks>
        /// Handles correct context management automatically.
        /// </remarks>
        /// <param name="action">The work to run.</param>
        /// <typeparam name="T">The return type.</typeparam>
        public T Run<T>(Func<Realm, T> action)
        {
            if (ThreadSafety.IsUpdateThread)
                return action(Context);

            using (var realm = createContext())
                return action(realm);
        }

        /// <summary>
        /// Run work on realm.
        /// </summary>
        /// <remarks>
        /// Handles correct context management automatically.
        /// </remarks>
        /// <param name="action">The work to run.</param>
        public void Run(Action<Realm> action)
        {
            if (ThreadSafety.IsUpdateThread)
                action(Context);
            else
            {
                using (var realm = createContext())
                    action(realm);
            }
        }

        /// <summary>
        /// Write changes to realm.
        /// </summary>
        /// <remarks>
        /// Handles correct context management and transaction committing automatically.
        /// </remarks>
        /// <param name="action">The work to run.</param>
        public void Write(Action<Realm> action)
        {
            if (ThreadSafety.IsUpdateThread)
                Context.Write(action);
            else
            {
                using (var realm = createContext())
                    realm.Write(action);
            }
        }

        private readonly Dictionary<Func<Realm, IDisposable?>, IDisposable?> customSubscriptionActions = new Dictionary<Func<Realm, IDisposable?>, IDisposable?>();

        private readonly Dictionary<Func<Realm, IDisposable?>, Action> realmSubscriptionsResetMap = new Dictionary<Func<Realm, IDisposable?>, Action>();

        public IDisposable Register<T>(Func<Realm, IQueryable<T>> query, NotificationCallbackDelegate<T> onChanged)
            where T : RealmObjectBase
        {
            if (!ThreadSafety.IsUpdateThread)
                throw new InvalidOperationException(@$"{nameof(Register)} must be called from the update thread.");

            realmSubscriptionsResetMap.Add(action, () => onChanged(new EmptyRealmSet<T>(), null, null));

            return Register(action);

            IDisposable? action(Realm realm) => query(realm).QueryAsyncWithNotifications(onChanged);
        }

        /// <summary>
        /// Run work on realm that will be run every time the update thread realm context gets recycled.
        /// </summary>
        /// <param name="action">The work to run. Return value should be an <see cref="IDisposable"/> from QueryAsyncWithNotifications, or an <see cref="InvokeOnDisposal"/> to clean up any bindings.</param>
        /// <returns>An <see cref="IDisposable"/> which should be disposed to unsubscribe any inner subscription.</returns>
        public IDisposable Register(Func<Realm, IDisposable?> action)
        {
            if (!ThreadSafety.IsUpdateThread)
                throw new InvalidOperationException(@$"{nameof(Register)} must be called from the update thread.");

            var syncContext = SynchronizationContext.Current;

            registerSubscription(action);

            // This token is returned to the consumer only.
            // It will cause the registration to be permanently removed.
            return new InvokeOnDisposal(() =>
            {
                if (ThreadSafety.IsUpdateThread)
                    unsubscribe();
                else
                    syncContext.Post(_ => unsubscribe(), null);

                void unsubscribe()
                {
                    lock (contextLock)
                    {
                        if (customSubscriptionActions.TryGetValue(action, out var unsubscriptionAction))
                        {
                            unsubscriptionAction?.Dispose();
                            customSubscriptionActions.Remove(action);
                            realmSubscriptionsResetMap.Remove(action);
                        }
                    }
                }
            });
        }

        private void registerSubscription(Func<Realm, IDisposable?> action)
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);

            // Retrieve context outside of flag update to ensure that the context is constructed,
            // as attempting to access it inside the subscription if it's not constructed would lead to
            // cyclic invocations of the subscription callback.
            var realm = Context;

            lock (contextLock)
            {
                Debug.Assert(!customSubscriptionActions.TryGetValue(action, out var found) || found == null);

                current_thread_subscriptions_allowed.Value = true;
                customSubscriptionActions[action] = action(realm);
                current_thread_subscriptions_allowed.Value = false;
            }
        }

        /// <summary>
        /// Unregister all subscriptions when the realm context is to be recycled.
        /// Subscriptions will still remain and will be re-subscribed when the realm context returns.
        /// </summary>
        private void unregisterAllSubscriptions()
        {
            foreach (var action in realmSubscriptionsResetMap.Values)
                action();

            foreach (var action in customSubscriptionActions)
            {
                action.Value?.Dispose();
                customSubscriptionActions[action.Key] = null;
            }
        }

        private Realm createContext()
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
            // This is currently the only usage of temporary files at the osu! side.
            // If we use the temporary folder in more situations in the future, this should be moved to a higher level (helper method or OsuGameBase).
            string tempPathLocation = Path.Combine(Path.GetTempPath(), @"lazer");
            if (!Directory.Exists(tempPathLocation))
                Directory.CreateDirectory(tempPathLocation);

            return new RealmConfiguration(storage.GetFullPath(Filename, true))
            {
                SchemaVersion = schema_version,
                MigrationCallback = onMigration,
                FallbackPipePath = tempPathLocation,
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
                    convertOnlineIDs<BeatmapInfo>();
                    convertOnlineIDs<BeatmapSetInfo>();
                    convertOnlineIDs<RulesetInfo>();

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
                    string metadataClassName = getMappedOrOriginalName(typeof(BeatmapMetadata));

                    // May be coming from a version before `RealmBeatmapMetadata` existed.
                    if (!migration.OldRealm.Schema.TryFindObjectSchema(metadataClassName, out _))
                        return;

                    var oldMetadata = migration.OldRealm.DynamicApi.All(metadataClassName);
                    var newMetadata = migration.NewRealm.All<BeatmapMetadata>();

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

        public void CreateBackup(string backupFilename)
        {
            using (BlockAllOperations())
            {
                Logger.Log($"Creating full realm database backup at {backupFilename}", LoggingTarget.Database);

                int attempts = 10;

                while (attempts-- > 0)
                {
                    try
                    {
                        using (var source = storage.GetStream(Filename))
                        using (var destination = storage.GetStream(backupFilename, FileAccess.Write, FileMode.CreateNew))
                            source.CopyTo(destination);
                        return;
                    }
                    catch (IOException)
                    {
                        // file may be locked during use.
                        Thread.Sleep(500);
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

            SynchronizationContext syncContext;

            try
            {
                contextCreationLock.Wait();

                lock (contextLock)
                {
                    if (!ThreadSafety.IsUpdateThread && context != null)
                        throw new InvalidOperationException(@$"{nameof(BlockAllOperations)} must be called from the update thread.");

                    syncContext = SynchronizationContext.Current;
                    unregisterAllSubscriptions();

                    Logger.Log(@"Blocking realm operations.", LoggingTarget.Database);

                    context?.Dispose();
                    context = null;
                }

                const int sleep_length = 200;
                int timeout = 5000;

                try
                {
                    // see https://github.com/realm/realm-dotnet/discussions/2657
                    while (!Compact())
                    {
                        Thread.Sleep(sleep_length);
                        timeout -= sleep_length;

                        if (timeout < 0)
                            throw new TimeoutException(@"Took too long to acquire lock");
                    }
                }
                catch (RealmException e)
                {
                    // Compact may fail if the realm is in a bad state.
                    // We still want to continue with the blocking operation, though.
                    Logger.Log($"Realm compact failed with error {e}", LoggingTarget.Database);
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

                // Post back to the update thread to revive any subscriptions.
                syncContext?.Post(_ => ensureUpdateContext(), null);
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
