// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Development;
using osu.Framework.Extensions;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Input.Bindings;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Skinning;
using osuTK.Input;
using Realms;
using Realms.Exceptions;

namespace osu.Game.Database
{
    /// <summary>
    /// A factory which provides safe access to the realm storage backend.
    /// </summary>
    public class RealmAccess : IDisposable
    {
        private readonly Storage storage;

        /// <summary>
        /// The filename of this realm.
        /// </summary>
        public readonly string Filename;

        private readonly SynchronizationContext? updateThreadSyncContext;

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
        /// 14   2022-03-01    Added BeatmapUserSettings to BeatmapInfo.
        /// 15   2022-07-13    Added LastPlayed to BeatmapInfo.
        /// 16   2022-07-15    Removed HasReplay from ScoreInfo.
        /// 17   2022-07-16    Added CountryCode to RealmUser.
        /// 18   2022-07-19    Added OnlineMD5Hash and LastOnlineUpdate to BeatmapInfo.
        /// 19   2022-07-19    Added DateSubmitted and DateRanked to BeatmapSetInfo.
        /// 20   2022-07-21    Added LastAppliedDifficultyVersion to RulesetInfo, changed default value of BeatmapInfo.StarRating to -1.
        /// 21   2022-07-27    Migrate collections to realm (BeatmapCollection).
        /// 22   2022-07-31    Added ModPreset.
        /// 23   2022-08-01    Added LastLocalUpdate to BeatmapInfo.
        /// 24   2022-08-22    Added MaximumStatistics to ScoreInfo.
        /// 25   2022-09-18    Remove skins to add with new naming.
        /// 26   2023-02-05    Added BeatmapHash to ScoreInfo.
        /// 27   2023-06-06    Added EditorTimestamp to BeatmapInfo.
        /// 28   2023-06-08    Added IsLegacyScore to ScoreInfo, parsed from replay files.
        /// 29   2023-06-12    Run migration of old lazer scores to be best-effort in the new scoring number space. No actual realm changes.
        /// 30   2023-06-16    Run migration of old lazer scores again. This time with more correct rounding considerations.
        /// 31   2023-06-26    Add Version and LegacyTotalScore to ScoreInfo, set Version to 30000002 and copy TotalScore into LegacyTotalScore for legacy scores.
        /// 32   2023-07-09    Populate legacy scores with the ScoreV2 mod (and restore TotalScore to the legacy total for such scores) using replay files.
        /// 33   2023-08-16    Reset default chat toggle key binding to avoid conflict with newly added leaderboard toggle key binding.
        /// 34   2023-08-21    Add BackgroundReprocessingFailed flag to ScoreInfo to track upgrade failures.
        /// 35   2023-10-16    Clear key combinations of keybindings that are assigned to more than one action in a given settings section.
        /// 36   2023-10-26    Add LegacyOnlineID to ScoreInfo. Move osu_scores_*_high IDs stored in OnlineID to LegacyOnlineID. Reset anomalous OnlineIDs.
        /// </summary>
        private const int schema_version = 36;

        /// <summary>
        /// Lock object which is held during <see cref="BlockAllOperations"/> sections, blocking realm retrieval during blocking periods.
        /// </summary>
        private readonly SemaphoreSlim realmRetrievalLock = new SemaphoreSlim(1);

        /// <summary>
        /// <c>true</c> when the current thread has already entered the <see cref="realmRetrievalLock"/>.
        /// </summary>
        private readonly ThreadLocal<bool> currentThreadHasRealmRetrievalLock = new ThreadLocal<bool>();

        /// <summary>
        /// Holds a map of functions registered via <see cref="RegisterCustomSubscription"/> and <see cref="RegisterForNotifications{T}"/> and a coinciding action which when triggered,
        /// will unregister the subscription from realm.
        ///
        /// Put another way, the key is an action which registers the subscription with realm. The returned <see cref="IDisposable"/> from the action is stored as the value and only
        /// used internally.
        ///
        /// Entries in this dictionary are only removed when a consumer signals that the subscription should be permanently ceased (via their own <see cref="IDisposable"/>).
        /// </summary>
        private readonly Dictionary<Func<Realm, IDisposable?>, IDisposable?> customSubscriptionsResetMap = new Dictionary<Func<Realm, IDisposable?>, IDisposable?>();

        /// <summary>
        /// Holds a map of functions registered via <see cref="RegisterForNotifications{T}"/> and a coinciding action which when triggered,
        /// fires a change set event with an empty collection. This is used to inform subscribers when the main realm instance gets recycled, and ensure they don't use invalidated
        /// managed realm objects from a previous firing.
        /// </summary>
        private readonly Dictionary<Func<Realm, IDisposable?>, Action> notificationsResetMap = new Dictionary<Func<Realm, IDisposable?>, Action>();

        private static readonly GlobalStatistic<int> realm_instances_created = GlobalStatistics.Get<int>(@"Realm", @"Instances (Created)");

        private static readonly GlobalStatistic<int> total_subscriptions = GlobalStatistics.Get<int>(@"Realm", @"Subscriptions");

        private static readonly GlobalStatistic<int> total_reads_update = GlobalStatistics.Get<int>(@"Realm", @"Reads (Update)");

        private static readonly GlobalStatistic<int> total_reads_async = GlobalStatistics.Get<int>(@"Realm", @"Reads (Async)");

        private static readonly GlobalStatistic<int> total_writes_update = GlobalStatistics.Get<int>(@"Realm", @"Writes (Update)");

        private static readonly GlobalStatistic<int> total_writes_async = GlobalStatistics.Get<int>(@"Realm", @"Writes (Async)");

        private Realm? updateRealm;

        /// <summary>
        /// Tracks whether a realm was ever fetched from this instance.
        /// After a fetch occurs, blocking operations will be guaranteed to restore any subscriptions.
        /// </summary>
        private bool hasInitialisedOnce;

        private bool isSendingNotificationResetEvents;

        public Realm Realm => ensureUpdateRealm();

        private const string realm_extension = @".realm";

        private Realm ensureUpdateRealm()
        {
            if (isSendingNotificationResetEvents)
                throw new InvalidOperationException("Cannot retrieve a realm context from a notification callback during a blocking operation.");

            if (!ThreadSafety.IsUpdateThread)
                throw new InvalidOperationException(@$"Use {nameof(getRealmInstance)} when performing realm operations from a non-update thread");

            if (updateRealm == null)
            {
                updateRealm = getRealmInstance();
                hasInitialisedOnce = true;

                Logger.Log(@$"Opened realm ""{updateRealm.Config.DatabasePath}"" at version {updateRealm.Config.SchemaVersion}");

                // Resubscribe any subscriptions
                foreach (var action in customSubscriptionsResetMap.Keys.ToArray())
                    registerSubscription(action);
            }

            Debug.Assert(updateRealm != null);

            return updateRealm;
        }

        internal static bool CurrentThreadSubscriptionsAllowed => current_thread_subscriptions_allowed.Value;

        private static readonly ThreadLocal<bool> current_thread_subscriptions_allowed = new ThreadLocal<bool>();

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="storage">The game storage which will be used to create the realm backing file.</param>
        /// <param name="filename">The filename to use for the realm backing file. A ".realm" extension will be added automatically if not specified.</param>
        /// <param name="updateThread">The game update thread, used to post realm operations into a thread-safe context.</param>
        public RealmAccess(Storage storage, string filename, GameThread? updateThread = null)
        {
            this.storage = storage;

            updateThreadSyncContext = updateThread?.SynchronizationContext ?? SynchronizationContext.Current;

            Filename = filename;

            if (!Filename.EndsWith(realm_extension, StringComparison.Ordinal))
                Filename += realm_extension;

#if DEBUG
            if (!DebugUtils.IsNUnitRunning)
                applyFilenameSchemaSuffix(ref Filename);
#endif

            // `prepareFirstRealmAccess()` triggers the first `getRealmInstance` call, which will implicitly run realm migrations and bring the schema up-to-date.
            using (var realm = prepareFirstRealmAccess())
                cleanupPendingDeletions(realm);
        }

        /// <summary>
        /// Some developers may be annoyed if a newer version migration (ie. caused by testing a pull request)
        /// cause their test database to be unusable with previous versions.
        /// To get around this, store development databases against their realm version.
        /// Note that this means changes made on newer realm versions will disappear.
        /// </summary>
        private void applyFilenameSchemaSuffix(ref string filename)
        {
            string originalFilename = filename;

            filename = getVersionedFilename(schema_version);

            // First check if the current realm version already exists...
            if (storage.Exists(filename))
                return;

            // Check for a previous version we can use as a base database to migrate from...
            for (int i = schema_version - 1; i >= 0; i--)
            {
                string previousFilename = getVersionedFilename(i);

                if (storage.Exists(previousFilename))
                {
                    copyPreviousVersion(previousFilename, filename);
                    return;
                }
            }

            // Finally, check for  a non-versioned file exists (aka before this method was added)...
            if (storage.Exists(originalFilename))
                copyPreviousVersion(originalFilename, filename);

            void copyPreviousVersion(string previousFilename, string newFilename)
            {
                using (var previous = storage.GetStream(previousFilename))
                using (var current = storage.CreateFileSafely(newFilename))
                {
                    Logger.Log(@$"Copying previous realm database {previousFilename} to {newFilename} for migration to schema version {schema_version}");
                    previous.CopyTo(current);
                }
            }

            string getVersionedFilename(int version) => originalFilename.Replace(realm_extension, $"_{version}{realm_extension}");
        }

        private void attemptRecoverFromFile(string recoveryFilename)
        {
            Logger.Log($@"Performing recovery from {recoveryFilename}", LoggingTarget.Database);

            // First check the user hasn't started to use the database that is in place..
            try
            {
                using (var realm = Realm.GetInstance(getConfiguration()))
                {
                    if (realm.All<ScoreInfo>().Any())
                    {
                        Logger.Log(@"Recovery aborted as the existing database has scores set already.", LoggingTarget.Database);
                        Logger.Log($@"To perform recovery, delete {OsuGameBase.CLIENT_DATABASE_FILENAME} while osu! is not running.", LoggingTarget.Database);
                        return;
                    }
                }
            }
            catch
            {
                // Even if reading the in place database fails, still attempt to recover.
            }

            // Then check that the database we are about to attempt recovery can actually be recovered on this version..
            try
            {
                using (Realm.GetInstance(getConfiguration(recoveryFilename)))
                {
                    // Don't need to do anything, just check that opening the realm works correctly.
                }
            }
            catch
            {
                Logger.Log(@"Recovery aborted as the newer version could not be loaded by this osu! version.", LoggingTarget.Database);
                return;
            }

            // For extra safety, also store the temporarily-used database which we are about to replace.
            createBackup($"{Filename.Replace(realm_extension, string.Empty)}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_newer_version_before_recovery{realm_extension}");

            storage.Delete(Filename);

            using (var inputStream = storage.GetStream(recoveryFilename))
            using (var outputStream = storage.CreateFileSafely(Filename))
                inputStream.CopyTo(outputStream);

            storage.Delete(recoveryFilename);
            Logger.Log(@"Recovery complete!", LoggingTarget.Database);
        }

        private Realm prepareFirstRealmAccess()
        {
            string newerVersionFilename = $"{Filename.Replace(realm_extension, string.Empty)}_newer_version{realm_extension}";

            // Attempt to recover a newer database version if available.
            if (storage.Exists(newerVersionFilename))
            {
                Logger.Log(@"A newer realm database has been found, attempting recovery...", LoggingTarget.Database);
                attemptRecoverFromFile(newerVersionFilename);
            }

            try
            {
                return getRealmInstance();
            }
            catch (Exception e)
            {
                // See https://github.com/realm/realm-core/blob/master/src%2Frealm%2Fobject-store%2Fobject_store.cpp#L1016-L1022
                // This is the best way we can detect a schema version downgrade.
                if (e.Message.StartsWith(@"Provided schema version", StringComparison.Ordinal))
                {
                    Logger.Error(e, "Your local database is too new to work with this version of osu!. Please close osu! and install the latest release to recover your data.");

                    // If a newer version database already exists, don't backup again. We can presume that the first backup is the one we care about.
                    if (!storage.Exists(newerVersionFilename))
                        createBackup(newerVersionFilename);
                }
                else
                {
                    Logger.Error(e, "Realm startup failed with unrecoverable error; starting with a fresh database. A backup of your database has been made.");
                    createBackup($"{Filename.Replace(realm_extension, string.Empty)}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_corrupt{realm_extension}");
                }

                storage.Delete(Filename);
                return getRealmInstance();
            }
        }

        private void cleanupPendingDeletions(Realm realm)
        {
            try
            {
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

                    var pendingDeletePresets = realm.All<ModPreset>().Where(s => s.DeletePending);

                    foreach (var s in pendingDeletePresets)
                        realm.Remove(s);

                    transaction.Commit();
                }

                // clean up files after dropping any pending deletions.
                // in the future we may want to only do this when the game is idle, rather than on every startup.
                new RealmFileStore(this, storage).Cleanup();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to clean up unused files. This is not critical but please report if it happens regularly.");
            }
        }

        /// <summary>
        /// Compact this realm.
        /// </summary>
        /// <returns></returns>
        public bool Compact()
        {
            try
            {
                return Realm.Compact(getConfiguration());
            }
            // Catch can be removed along with entity framework. Is specifically to allow a failure message to arrive to the user (see similar catches in EFToRealmMigrator).
            catch (AggregateException ae) when (RuntimeInfo.OS == RuntimeInfo.Platform.macOS && ae.Flatten().InnerException is TypeInitializationException)
            {
                return true;
            }
        }

        /// <summary>
        /// Run work on realm with a return value.
        /// </summary>
        /// <param name="action">The work to run.</param>
        /// <typeparam name="T">The return type.</typeparam>
        public T Run<T>(Func<Realm, T> action)
        {
            if (ThreadSafety.IsUpdateThread)
            {
                total_reads_update.Value++;
                return action(Realm);
            }

            total_reads_async.Value++;
            using (var realm = getRealmInstance())
                return action(realm);
        }

        /// <summary>
        /// Run work on realm.
        /// </summary>
        /// <param name="action">The work to run.</param>
        public void Run(Action<Realm> action)
        {
            if (ThreadSafety.IsUpdateThread)
            {
                total_reads_update.Value++;
                action(Realm);
            }
            else
            {
                total_reads_async.Value++;
                using (var realm = getRealmInstance())
                    action(realm);
            }
        }

        /// <summary>
        /// Write changes to realm.
        /// </summary>
        /// <param name="action">The work to run.</param>
        public T Write<T>(Func<Realm, T> action)
        {
            if (ThreadSafety.IsUpdateThread)
            {
                total_writes_update.Value++;
                return Realm.Write(action);
            }
            else
            {
                total_writes_async.Value++;

                using (var realm = getRealmInstance())
                    return realm.Write(action);
            }
        }

        /// <summary>
        /// Write changes to realm.
        /// </summary>
        /// <param name="action">The work to run.</param>
        public void Write(Action<Realm> action)
        {
            if (ThreadSafety.IsUpdateThread)
            {
                total_writes_update.Value++;
                Realm.Write(action);
            }
            else
            {
                total_writes_async.Value++;

                using (var realm = getRealmInstance())
                    realm.Write(action);
            }
        }

        private readonly CountdownEvent pendingAsyncWrites = new CountdownEvent(0);

        /// <summary>
        /// Write changes to realm asynchronously, guaranteeing order of execution.
        /// </summary>
        /// <param name="action">The work to run.</param>
        public Task WriteAsync(Action<Realm> action)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RealmAccess));

            // Required to ensure the write is tracked and accounted for before disposal.
            // Can potentially be avoided if we have a need to do so in the future.
            if (!ThreadSafety.IsUpdateThread)
                throw new InvalidOperationException(@$"{nameof(WriteAsync)} must be called from the update thread.");

            // CountdownEvent will fail if already at zero.
            if (!pendingAsyncWrites.TryAddCount())
                pendingAsyncWrites.Reset(1);

            // Regardless of calling Realm.GetInstance or Realm.GetInstanceAsync, there is a blocking overhead on retrieval.
            // Adding a forced Task.Run resolves this.
            var writeTask = Task.Run(async () =>
            {
                total_writes_async.Value++;

                // Not attempting to use Realm.GetInstanceAsync as there's seemingly no benefit to us (for now) and it adds complexity due to locking
                // concerns in getRealmInstance(). On a quick check, it looks to be more suited to cases where realm is connecting to an online sync
                // server, which we don't use. May want to report upstream or revisit in the future.
                using (var realm = getRealmInstance())
                    // ReSharper disable once AccessToDisposedClosure (WriteAsync should be marked as [InstantHandle]).
                    await realm.WriteAsync(() => action(realm)).ConfigureAwait(false);

                pendingAsyncWrites.Signal();
            });

            return writeTask;
        }

        /// <summary>
        /// Subscribe to a realm collection and begin watching for asynchronous changes.
        /// </summary>
        /// <remarks>
        /// This adds osu! specific thread and managed state safety checks on top of <see cref="IRealmCollection{T}.SubscribeForNotifications"/>.
        ///
        /// In addition to the documented realm behaviour, we have the additional requirement of handling subscriptions over potential realm instance recycle.
        /// When this happens, callback events will be automatically fired:
        /// - On recycle start, a callback with an empty collection and <c>null</c> <see cref="ChangeSet"/> will be invoked.
        /// - On recycle end, a standard initial realm callback will arrive, with <c>null</c> <see cref="ChangeSet"/> and an up-to-date collection.
        /// </remarks>
        /// <param name="query">The <see cref="IQueryable{T}"/> to observe for changes.</param>
        /// <typeparam name="T">Type of the elements in the list.</typeparam>
        /// <param name="callback">The callback to be invoked with the updated <see cref="IRealmCollection{T}"/>.</param>
        /// <returns>
        /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
        /// To stop receiving notifications, call <see cref="IDisposable.Dispose"/>.
        /// </returns>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
        public IDisposable RegisterForNotifications<T>(Func<Realm, IQueryable<T>> query, NotificationCallbackDelegate<T> callback)
            where T : RealmObjectBase
        {
            Func<Realm, IDisposable?> action = realm => query(realm).QueryAsyncWithNotifications(callback);

            lock (notificationsResetMap)
            {
                // Store an action which is used when blocking to ensure consumers don't use results of a stale changeset firing.
                notificationsResetMap.Add(action, () => callback(new EmptyRealmSet<T>(), null));
            }

            return RegisterCustomSubscription(action);
        }

        /// <summary>
        /// Subscribe to the property of a realm object to watch for changes.
        /// </summary>
        /// <remarks>
        /// On subscribing, unless the <paramref name="modelAccessor"/> does not match an object, an initial invocation of <paramref name="onChanged"/> will occur immediately.
        /// Further invocations will occur when the value changes, but may also fire on a realm recycle with no actual value change.
        /// </remarks>
        /// <param name="modelAccessor">A function to retrieve the relevant model from realm.</param>
        /// <param name="propertyLookup">A function to traverse to the relevant property from the model.</param>
        /// <param name="onChanged">A function to be invoked when a change of value occurs.</param>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The type of the property to be watched.</typeparam>
        /// <returns>
        /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
        /// To stop receiving notifications, call <see cref="IDisposable.Dispose"/>.
        /// </returns>
        public IDisposable SubscribeToPropertyChanged<TModel, TProperty>(Func<Realm, TModel?> modelAccessor, Expression<Func<TModel, TProperty>> propertyLookup, Action<TProperty> onChanged)
            where TModel : RealmObjectBase
        {
            return RegisterCustomSubscription(_ =>
            {
                string propertyName = getMemberName(propertyLookup);

                var model = Run(modelAccessor);
                var propLookupCompiled = propertyLookup.Compile();

                if (model == null)
                    return null;

                model.PropertyChanged += onPropertyChanged;

                // Update initial value immediately.
                onChanged(propLookupCompiled(model));

                return new InvokeOnDisposal(() => model.PropertyChanged -= onPropertyChanged);

                void onPropertyChanged(object? sender, PropertyChangedEventArgs args)
                {
                    if (args.PropertyName == propertyName)
                        onChanged(propLookupCompiled(model));
                }
            });

            static string getMemberName(Expression<Func<TModel, TProperty>> expression)
            {
                if (!(expression is LambdaExpression lambda))
                    throw new ArgumentException("Outermost expression must be a lambda expression", nameof(expression));

                if (!(lambda.Body is MemberExpression memberExpression))
                    throw new ArgumentException("Lambda body must be a member access expression", nameof(expression));

                // TODO: nested access can be supported, with more iteration here
                // (need to iteratively soft-cast `memberExpression.Expression` into `MemberExpression`s until `lambda.Parameters[0]` is hit)
                if (memberExpression.Expression != lambda.Parameters[0])
                    throw new ArgumentException("Nested access expressions are not supported", nameof(expression));

                return memberExpression.Member.Name;
            }
        }

        /// <summary>
        /// Run work on realm that will be run every time the update thread realm instance gets recycled.
        /// </summary>
        /// <param name="action">The work to run. Return value should be an <see cref="IDisposable"/> from QueryAsyncWithNotifications, or an <see cref="InvokeOnDisposal"/> to clean up any bindings.</param>
        /// <returns>An <see cref="IDisposable"/> which should be disposed to unsubscribe any inner subscription.</returns>
        public IDisposable RegisterCustomSubscription(Func<Realm, IDisposable?> action)
        {
            if (updateThreadSyncContext == null)
                throw new InvalidOperationException("Attempted to register a realm subscription before update thread registration.");

            total_subscriptions.Value++;

            if (ThreadSafety.IsUpdateThread)
                updateThreadSyncContext.Send(_ => registerSubscription(action), null);
            else
                updateThreadSyncContext.Post(_ => registerSubscription(action), null);

            // This token is returned to the consumer.
            // When disposed, it will cause the registration to be permanently ceased (unsubscribed with realm and unregistered by this class).
            return new InvokeOnDisposal(() =>
            {
                if (ThreadSafety.IsUpdateThread)
                    updateThreadSyncContext.Send(_ => unsubscribe(), null);
                else
                    updateThreadSyncContext.Post(_ => unsubscribe(), null);

                void unsubscribe()
                {
                    if (customSubscriptionsResetMap.TryGetValue(action, out var unsubscriptionAction))
                    {
                        unsubscriptionAction?.Dispose();
                        customSubscriptionsResetMap.Remove(action);

                        lock (notificationsResetMap)
                        {
                            notificationsResetMap.Remove(action);
                        }

                        total_subscriptions.Value--;
                    }
                }
            });
        }

        private void registerSubscription(Func<Realm, IDisposable?> action)
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);

            // Retrieve realm instance outside of flag update to ensure that the instance is retrieved,
            // as attempting to access it inside the subscription if it's not constructed would lead to
            // cyclic invocations of the subscription callback.
            var realm = Realm;

            Debug.Assert(!customSubscriptionsResetMap.TryGetValue(action, out var found) || found == null);

            current_thread_subscriptions_allowed.Value = true;
            customSubscriptionsResetMap[action] = action(realm);
            current_thread_subscriptions_allowed.Value = false;
        }

        private Realm getRealmInstance()
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RealmAccess));

            bool tookSemaphoreLock = false;

            try
            {
                // Ensure that the thread that currently has the `realmRetrievalLock` can retrieve nested contexts and not deadlock on itself.
                if (!currentThreadHasRealmRetrievalLock.Value)
                {
                    realmRetrievalLock.Wait();
                    currentThreadHasRealmRetrievalLock.Value = true;
                    tookSemaphoreLock = true;
                }
                else
                {
                    // the semaphore is used to handle blocking of all realm retrieval during certain periods.
                    // once the semaphore has been taken by this code section, it is safe to retrieve further realm instances on the same thread.
                    // this can happen if a realm subscription is active and triggers a callback which has user code that calls `Run`.
                }

                realm_instances_created.Value++;

                return Realm.GetInstance(getConfiguration());
            }
            // Catch can be removed along with entity framework. Is specifically to allow a failure message to arrive to the user (see similar catches in EFToRealmMigrator).
            catch (AggregateException ae) when (RuntimeInfo.OS == RuntimeInfo.Platform.macOS && ae.Flatten().InnerException is TypeInitializationException)
            {
                return Realm.GetInstance();
            }
            finally
            {
                if (tookSemaphoreLock)
                {
                    realmRetrievalLock.Release();
                    currentThreadHasRealmRetrievalLock.Value = false;
                }
            }
        }

        private RealmConfiguration getConfiguration(string? filename = null)
        {
            // This is currently the only usage of temporary files at the osu! side.
            // If we use the temporary folder in more situations in the future, this should be moved to a higher level (helper method or OsuGameBase).
            string tempPathLocation = Path.Combine(Path.GetTempPath(), @"lazer");
            if (!Directory.Exists(tempPathLocation))
                Directory.CreateDirectory(tempPathLocation);

            return new RealmConfiguration(storage.GetFullPath(filename ?? Filename, true))
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
            Logger.Log($"Running realm migration to version {targetVersion}...");
            Stopwatch stopwatch = new Stopwatch();

            var files = new RealmFileStore(this, storage);

            stopwatch.Start();

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
                            dynamic oldItem = oldItems.ElementAt(i);
                            dynamic newItem = newItems.ElementAt(i);

                            long? nullableOnlineID = oldItem.OnlineID;
                            newItem.OnlineID = (int)(nullableOnlineID ?? -1);
                        }
                    }

                    break;

                case 8:
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

                    break;
                }

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
                        dynamic oldItem = oldMetadata.ElementAt(i);
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
                        dynamic oldItem = oldSettings.ElementAt(i);
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
                {
                    string keyBindingClassName = getMappedOrOriginalName(typeof(RealmKeyBinding));

                    if (!migration.OldRealm.Schema.TryFindObjectSchema(keyBindingClassName, out _))
                        return;

                    var oldKeyBindings = migration.OldRealm.DynamicApi.All(keyBindingClassName);
                    var newKeyBindings = migration.NewRealm.All<RealmKeyBinding>().ToList();

                    for (int i = 0; i < newKeyBindings.Count; i++)
                    {
                        dynamic oldItem = oldKeyBindings.ElementAt(i);
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

                case 14:
                    foreach (var beatmap in migration.NewRealm.All<BeatmapInfo>())
                        beatmap.UserSettings = new BeatmapUserSettings();

                    break;

                case 20:
                    // As we now have versioned difficulty calculations, let's reset
                    // all star ratings and have `BackgroundBeatmapProcessor` recalculate them.
                    foreach (var beatmap in migration.NewRealm.All<BeatmapInfo>())
                        beatmap.StarRating = -1;

                    break;

                case 21:
                    // Migrate collections from external file to inside realm.
                    // We use the "legacy" importer because that is how things were actually being saved out until now.
                    var legacyCollectionImporter = new LegacyCollectionImporter(this);

                    if (legacyCollectionImporter.GetAvailableCount(storage).GetResultSafely() > 0)
                    {
                        legacyCollectionImporter.ImportFromStorage(storage).ContinueWith(_ => storage.Move("collection.db", "collection.db.migrated"));
                    }

                    break;

                case 25:
                    // Remove the default skins so they can be added back by SkinManager with updated naming.
                    migration.NewRealm.RemoveRange(migration.NewRealm.All<SkinInfo>().Where(s => s.Protected));
                    break;

                case 26:
                {
                    // Add ScoreInfo.BeatmapHash property to ensure scores correspond to the correct version of beatmap.
                    var scores = migration.NewRealm.All<ScoreInfo>();

                    foreach (var score in scores)
                        score.BeatmapHash = score.BeatmapInfo?.Hash ?? string.Empty;

                    break;
                }

                case 28:
                {
                    var scores = migration.NewRealm.All<ScoreInfo>();

                    foreach (var score in scores)
                    {
                        score.PopulateFromReplay(files, sr =>
                        {
                            sr.ReadByte(); // Ruleset.
                            int version = sr.ReadInt32();
                            if (version < LegacyScoreEncoder.FIRST_LAZER_VERSION)
                                score.IsLegacyScore = true;
                        });
                    }

                    break;
                }

                case 29:
                case 30:
                {
                    var scores = migration.NewRealm
                                          .All<ScoreInfo>()
                                          .Where(s => !s.IsLegacyScore);

                    foreach (var score in scores)
                    {
                        try
                        {
                            if (StandardisedScoreMigrationTools.ShouldMigrateToNewStandardised(score))
                            {
                                try
                                {
                                    long calculatedNew = StandardisedScoreMigrationTools.GetNewStandardised(score);
                                    score.TotalScore = calculatedNew;
                                }
                                catch
                                {
                                }
                            }
                        }
                        catch { }
                    }

                    break;
                }

                case 31:
                {
                    foreach (var score in migration.NewRealm.All<ScoreInfo>())
                    {
                        if (score.IsLegacyScore && score.Ruleset.IsLegacyRuleset())
                        {
                            // Scores with this version will trigger the score upgrade process in BackgroundBeatmapProcessor.
                            score.TotalScoreVersion = 30000002;

                            // Transfer known legacy scores to a permanent storage field for preservation.
                            score.LegacyTotalScore = score.TotalScore;
                        }
                        else
                            score.TotalScoreVersion = LegacyScoreEncoder.LATEST_VERSION;
                    }

                    break;
                }

                case 32:
                {
                    foreach (var score in migration.NewRealm.All<ScoreInfo>())
                    {
                        if (!score.IsLegacyScore || !score.Ruleset.IsLegacyRuleset())
                            continue;

                        score.PopulateFromReplay(files, sr =>
                        {
                            sr.ReadByte(); // Ruleset.
                            sr.ReadInt32(); // Version.
                            sr.ReadString(); // Beatmap hash.
                            sr.ReadString(); // Username.
                            sr.ReadString(); // MD5Hash.
                            sr.ReadUInt16(); // Count300.
                            sr.ReadUInt16(); // Count100.
                            sr.ReadUInt16(); // Count50.
                            sr.ReadUInt16(); // CountGeki.
                            sr.ReadUInt16(); // CountKatu.
                            sr.ReadUInt16(); // CountMiss.

                            // we should have this in LegacyTotalScore already, but if we're reading through this anyways...
                            int totalScore = sr.ReadInt32();

                            sr.ReadUInt16(); // Max combo.
                            sr.ReadBoolean(); // Perfect.

                            var legacyMods = (LegacyMods)sr.ReadInt32();

                            if (!legacyMods.HasFlagFast(LegacyMods.ScoreV2) || score.APIMods.Any(mod => mod.Acronym == @"SV2"))
                                return;

                            score.APIMods = score.APIMods.Append(new APIMod(new ModScoreV2())).ToArray();
                            score.LegacyTotalScore = score.TotalScore = totalScore;
                        });
                    }

                    break;
                }

                case 33:
                {
                    // Clear default bindings for the chat focus toggle,
                    // as they would conflict with the newly-added leaderboard toggle.
                    var keyBindings = migration.NewRealm.All<RealmKeyBinding>();

                    var toggleChatBind = keyBindings.FirstOrDefault(bind => bind.ActionInt == (int)GlobalAction.ToggleChatFocus);
                    if (toggleChatBind != null && toggleChatBind.KeyCombination.Keys.SequenceEqual(new[] { InputKey.Tab }))
                        migration.NewRealm.Remove(toggleChatBind);

                    break;
                }

                case 35:
                {
                    // catch used `Shift` twice as a default key combination for dash, which generally was bothersome and causes issues elsewhere.
                    // the duplicate binding logic below had to account for it, it could also break keybinding conflict resolution on revert-to-default.
                    // as such, detect this situation and fix it before proceeding further.
                    var catchDashBindings = migration.NewRealm.All<RealmKeyBinding>()
                                                     .Where(kb => kb.RulesetName == @"fruits" && kb.ActionInt == 2)
                                                     .ToList();

                    if (catchDashBindings.All(kb => kb.KeyCombination.Equals(new KeyCombination(InputKey.Shift))))
                    {
                        Debug.Assert(catchDashBindings.Count == 2);
                        catchDashBindings.Last().KeyCombination = KeyCombination.FromMouseButton(MouseButton.Left);
                    }

                    // with the catch case dealt with, de-duplicate the remaining bindings.
                    int countCleared = 0;

                    var globalBindings = migration.NewRealm.All<RealmKeyBinding>().Where(kb => kb.RulesetName == null).ToList();

                    foreach (var category in Enum.GetValues<GlobalActionCategory>())
                    {
                        var categoryActions = GlobalActionContainer.GetGlobalActionsFor(category).Cast<int>().ToHashSet();
                        var categoryBindings = globalBindings.Where(kb => categoryActions.Contains(kb.ActionInt));
                        countCleared += RealmKeyBindingStore.ClearDuplicateBindings(categoryBindings);
                    }

                    var rulesetBindings = migration.NewRealm.All<RealmKeyBinding>().Where(kb => kb.RulesetName != null).ToList();

                    foreach (var variantGroup in rulesetBindings.GroupBy(kb => (kb.RulesetName, kb.Variant)))
                        countCleared += RealmKeyBindingStore.ClearDuplicateBindings(variantGroup);

                    if (countCleared > 0)
                    {
                        Logger.Log($"{countCleared} of your keybinding(s) have been cleared due to being bound to multiple actions. "
                                   + "Please choose new unique ones in the settings panel.", level: LogLevel.Important);
                    }

                    break;
                }

                case 36:
                {
                    foreach (var score in migration.NewRealm.All<ScoreInfo>())
                    {
                        if (score.OnlineID > 0)
                        {
                            score.LegacyOnlineID = score.OnlineID;
                            score.OnlineID = -1;
                        }
                        else
                        {
                            score.LegacyOnlineID = score.OnlineID = -1;
                        }
                    }

                    break;
                }
            }

            Logger.Log($"Migration completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        private string? getRulesetShortNameFromLegacyID(long rulesetId)
        {
            try
            {
                return new APIBeatmap.APIRuleset { OnlineID = (int)rulesetId }.ShortName;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Create a full realm backup.
        /// </summary>
        /// <param name="backupFilename">The filename for the backup.</param>
        public void CreateBackup(string backupFilename)
        {
            if (realmRetrievalLock.CurrentCount != 0)
                throw new InvalidOperationException($"Call {nameof(BlockAllOperations)} before creating a backup.");

            createBackup(backupFilename);
        }

        private void createBackup(string backupFilename)
        {
            Logger.Log($"Creating full realm database backup at {backupFilename}", LoggingTarget.Database);

            int attempts = 10;

            while (true)
            {
                try
                {
                    using (var source = storage.GetStream(Filename, mode: FileMode.Open))
                    {
                        // source may not exist.
                        if (source == null)
                            return;

                        using (var destination = storage.GetStream(backupFilename, FileAccess.Write, FileMode.CreateNew))
                            source.CopyTo(destination);
                    }

                    return;
                }
                catch (IOException)
                {
                    if (attempts-- <= 0)
                        throw;

                    // file may be locked during use.
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// Flush any active realm instances and block any further writes.
        /// </summary>
        /// <remarks>
        /// This should be used in places we need to ensure no ongoing reads/writes are occurring with realm.
        /// ie. to move the realm backing file to a new location.
        /// </remarks>
        /// <param name="reason">The reason for blocking. Used for logging purposes.</param>
        /// <returns>An <see cref="IDisposable"/> which should be disposed to end the blocking section.</returns>
        public IDisposable BlockAllOperations(string reason)
        {
            Logger.Log($@"Attempting to block all realm operations for {reason}.", LoggingTarget.Database);

            if (!ThreadSafety.IsUpdateThread)
                throw new InvalidOperationException(@$"{nameof(BlockAllOperations)} must be called from the update thread.");

            if (isDisposed)
                throw new ObjectDisposedException(nameof(RealmAccess));

            SynchronizationContext? syncContext = null;

            try
            {
                realmRetrievalLock.Wait();

                if (hasInitialisedOnce)
                {
                    syncContext = SynchronizationContext.Current;

                    // Before disposing the update context, clean up all subscriptions.
                    // Note that in the case of realm notification subscriptions, this is not really required (they will be cleaned up by disposal).
                    // In the case of custom subscriptions, we want them to fire before the update realm is disposed in case they do any follow-up work.
                    foreach (var action in customSubscriptionsResetMap.ToArray())
                    {
                        action.Value?.Dispose();
                        customSubscriptionsResetMap[action.Key] = null;
                    }

                    updateRealm?.Dispose();
                    updateRealm = null;
                }

                Logger.Log(@"Lock acquired for blocking operations", LoggingTarget.Database);

                const int sleep_length = 200;
                int timeSpent = 0;

                try
                {
                    // see https://github.com/realm/realm-dotnet/discussions/2657
                    while (!Compact())
                    {
                        Thread.Sleep(sleep_length);
                        timeSpent += sleep_length;

                        if (timeSpent > 5000)
                            throw new TimeoutException($@"Realm compact failed after {timeSpent / sleep_length} attempts over {timeSpent / 1000} seconds");
                    }
                }
                catch (RealmException e)
                {
                    // Compact may fail if the realm is in a bad state.
                    // We still want to continue with the blocking operation, though.
                    Logger.Log($"Realm compact failed with error {e}", LoggingTarget.Database);
                }

                Logger.Log(@"Realm usage isolated via compact", LoggingTarget.Database);

                // In order to ensure events arrive in the correct order, these *must* be fired post disposal of the update realm,
                // and must be posted to the synchronization context.
                // This is because realm may fire event callbacks between the `unregisterAllSubscriptions` and `updateRealm.Dispose`
                // calls above.
                syncContext?.Send(_ =>
                {
                    // Flag ensures that we don't get in a deadlocked scenario due to a callback attempting to access `RealmAccess.Realm` or `RealmAccess.Run`
                    // and hitting `realmRetrievalLock` a second time. Generally such usages should not exist, and as such we throw when an attempt is made
                    // to use in this fashion.
                    isSendingNotificationResetEvents = true;

                    try
                    {
                        lock (notificationsResetMap)
                        {
                            foreach (var action in notificationsResetMap.Values)
                                action();
                        }
                    }
                    finally
                    {
                        isSendingNotificationResetEvents = false;
                    }
                }, null);
            }
            catch
            {
                restoreOperation();
                throw;
            }

            return new InvokeOnDisposal(restoreOperation);

            void restoreOperation()
            {
                // Release of lock needs to happen here rather than on the update thread, as there may be another
                // operation already blocking the update thread waiting for the blocking operation to complete.
                Logger.Log(@"Restoring realm operations.", LoggingTarget.Database);
                realmRetrievalLock.Release();

                if (syncContext == null) return;

                ManualResetEventSlim updateRealmReestablished = new ManualResetEventSlim();

                // Post back to the update thread to revive any subscriptions.
                // In the case we are on the update thread, let's also require this to run synchronously.
                // This requirement is mostly due to test coverage, but shouldn't cause any harm.
                if (ThreadSafety.IsUpdateThread)
                {
                    syncContext.Send(_ =>
                    {
                        ensureUpdateRealm();
                        updateRealmReestablished.Set();
                    }, null);
                }
                else
                {
                    syncContext.Post(_ =>
                    {
                        ensureUpdateRealm();
                        updateRealmReestablished.Set();
                    }, null);
                }

                // Wait for the post to complete to ensure a second `Migrate` operation doesn't start in the mean time.
                // This is important to ensure `ensureUpdateRealm` is run before another blocking migration operation starts.
                if (!updateRealmReestablished.Wait(10000))
                    throw new TimeoutException(@"Reestablishing update realm after block took too long");
            }
        }

        // https://github.com/realm/realm-dotnet/blob/32f4ebcc88b3e80a3b254412665340cd9f3bd6b5/Realm/Realm/Extensions/ReflectionExtensions.cs#L46
        private static string getMappedOrOriginalName(MemberInfo member) => member.GetCustomAttribute<MapToAttribute>()?.Mapping ?? member.Name;

        private bool isDisposed;

        public void Dispose()
        {
            if (!pendingAsyncWrites.Wait(10000))
                Logger.Log("Realm took too long waiting on pending async writes", level: LogLevel.Error);

            updateRealm?.Dispose();

            if (!isDisposed)
            {
                // intentionally block realm retrieval indefinitely. this ensures that nothing can start consuming a new instance after disposal.
                realmRetrievalLock.Wait();
                realmRetrievalLock.Dispose();

                isDisposed = true;
            }
        }
    }
}
