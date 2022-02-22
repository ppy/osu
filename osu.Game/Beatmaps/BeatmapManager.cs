// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osu.Game.Stores;
using osu.Game.Utils;

#nullable enable

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles general operations related to global beatmap management.
    /// </summary>
    [ExcludeFromDynamicCompile]
    public class BeatmapManager : IModelManager<BeatmapSetInfo>, IModelFileManager<BeatmapSetInfo, RealmNamedFileUsage>, IModelImporter<BeatmapSetInfo>, IWorkingBeatmapCache, IDisposable
    {
        public ITrackStore BeatmapTrackStore { get; }

        private readonly BeatmapModelManager beatmapModelManager;

        private readonly WorkingBeatmapCache workingBeatmapCache;
        private readonly BeatmapOnlineLookupQueue? onlineBeatmapLookupQueue;

        private readonly RealmAccess realm;

        public BeatmapManager(Storage storage, RealmAccess realm, RulesetStore rulesets, IAPIProvider? api, AudioManager audioManager, IResourceStore<byte[]> gameResources, GameHost? host = null, WorkingBeatmap? defaultBeatmap = null, bool performOnlineLookups = false)
        {
            this.realm = realm;

            if (performOnlineLookups)
            {
                if (api == null)
                    throw new ArgumentNullException(nameof(api), "API must be provided if online lookups are required.");

                onlineBeatmapLookupQueue = new BeatmapOnlineLookupQueue(api, storage);
            }

            var userResources = new RealmFileStore(realm, storage).Store;

            BeatmapTrackStore = audioManager.GetTrackStore(userResources);

            beatmapModelManager = CreateBeatmapModelManager(storage, realm, rulesets, onlineBeatmapLookupQueue);
            workingBeatmapCache = CreateWorkingBeatmapCache(audioManager, gameResources, userResources, defaultBeatmap, host);

            beatmapModelManager.WorkingBeatmapCache = workingBeatmapCache;
        }

        protected virtual WorkingBeatmapCache CreateWorkingBeatmapCache(AudioManager audioManager, IResourceStore<byte[]> resources, IResourceStore<byte[]> storage, WorkingBeatmap? defaultBeatmap, GameHost? host)
        {
            return new WorkingBeatmapCache(BeatmapTrackStore, audioManager, resources, storage, defaultBeatmap, host);
        }

        protected virtual BeatmapModelManager CreateBeatmapModelManager(Storage storage, RealmAccess realm, RulesetStore rulesets, BeatmapOnlineLookupQueue? onlineLookupQueue) =>
            new BeatmapModelManager(realm, storage, onlineLookupQueue);

        /// <summary>
        /// Create a new beatmap set, backed by a <see cref="BeatmapSetInfo"/> model,
        /// with a single difficulty which is backed by a <see cref="BeatmapInfo"/> model
        /// and represented by the returned usable <see cref="WorkingBeatmap"/>.
        /// </summary>
        public WorkingBeatmap CreateNew(RulesetInfo ruleset, APIUser user)
        {
            var metadata = new BeatmapMetadata
            {
                Author = new RealmUser
                {
                    OnlineID = user.OnlineID,
                    Username = user.Username,
                }
            };

            var beatmapSet = new BeatmapSetInfo
            {
                Beatmaps =
                {
                    new BeatmapInfo(ruleset, new BeatmapDifficulty(), metadata)
                }
            };

            foreach (BeatmapInfo b in beatmapSet.Beatmaps)
                b.BeatmapSet = beatmapSet;

            var imported = beatmapModelManager.Import(beatmapSet);

            if (imported == null)
                throw new InvalidOperationException("Failed to import new beatmap");

            return imported.PerformRead(s => GetWorkingBeatmap(s.Beatmaps.First()));
        }

        /// <summary>
        /// Add a new difficulty to the provided <paramref name="targetBeatmapSet"/> based on the provided <paramref name="referenceWorkingBeatmap"/>.
        /// The new difficulty will be backed by a <see cref="BeatmapInfo"/> model
        /// and represented by the returned <see cref="WorkingBeatmap"/>.
        /// </summary>
        /// <remarks>
        /// Contrary to <see cref="CopyExistingDifficulty"/>, this method does not preserve hitobjects and beatmap-level settings from <paramref name="referenceWorkingBeatmap"/>.
        /// The created beatmap will have zero hitobjects and will have default settings (including difficulty settings), but will preserve metadata and existing timing points.
        /// </remarks>
        /// <param name="targetBeatmapSet">The <see cref="BeatmapSetInfo"/> to add the new difficulty to.</param>
        /// <param name="referenceWorkingBeatmap">The <see cref="WorkingBeatmap"/> to use as a baseline reference when creating the new difficulty.</param>
        /// <param name="rulesetInfo">The ruleset with which the new difficulty should be created.</param>
        public virtual WorkingBeatmap CreateNewDifficulty(BeatmapSetInfo targetBeatmapSet, WorkingBeatmap referenceWorkingBeatmap, RulesetInfo rulesetInfo)
        {
            var playableBeatmap = referenceWorkingBeatmap.GetPlayableBeatmap(rulesetInfo);

            var newBeatmapInfo = new BeatmapInfo(rulesetInfo, new BeatmapDifficulty(), playableBeatmap.Metadata.DeepClone())
            {
                DifficultyName = NamingUtils.GetNextBestName(targetBeatmapSet.Beatmaps.Select(b => b.DifficultyName), "New Difficulty")
            };
            var newBeatmap = new Beatmap { BeatmapInfo = newBeatmapInfo };
            foreach (var timingPoint in playableBeatmap.ControlPointInfo.TimingPoints)
                newBeatmap.ControlPointInfo.Add(timingPoint.Time, timingPoint.DeepClone());

            return addDifficultyToSet(targetBeatmapSet, newBeatmap, referenceWorkingBeatmap.Skin);
        }

        /// <summary>
        /// Add a copy of the provided <paramref name="referenceWorkingBeatmap"/> to the provided <paramref name="targetBeatmapSet"/>.
        /// The new difficulty will be backed by a <see cref="BeatmapInfo"/> model
        /// and represented by the returned <see cref="WorkingBeatmap"/>.
        /// </summary>
        /// <remarks>
        /// Contrary to <see cref="CreateNewDifficulty"/>, this method creates a nearly-exact copy of <paramref name="referenceWorkingBeatmap"/>
        /// (with the exception of a few key properties that cannot be copied under any circumstance, like difficulty name, beatmap hash, or online status).
        /// </remarks>
        /// <param name="targetBeatmapSet">The <see cref="BeatmapSetInfo"/> to add the copy to.</param>
        /// <param name="referenceWorkingBeatmap">The <see cref="WorkingBeatmap"/> to be copied.</param>
        public virtual WorkingBeatmap CopyExistingDifficulty(BeatmapSetInfo targetBeatmapSet, WorkingBeatmap referenceWorkingBeatmap)
        {
            var newBeatmap = referenceWorkingBeatmap.GetPlayableBeatmap(referenceWorkingBeatmap.BeatmapInfo.Ruleset).Clone();
            BeatmapInfo newBeatmapInfo;

            newBeatmap.BeatmapInfo = newBeatmapInfo = referenceWorkingBeatmap.BeatmapInfo.Clone();
            // assign a new ID to the clone.
            newBeatmapInfo.ID = Guid.NewGuid();
            // add "(copy)" suffix to difficulty name, and additionally ensure that it doesn't conflict with any other potentially pre-existing copies.
            newBeatmapInfo.DifficultyName = NamingUtils.GetNextBestName(
                targetBeatmapSet.Beatmaps.Select(b => b.DifficultyName),
                $"{newBeatmapInfo.DifficultyName} (copy)");
            // clear the hash, as that's what is used to match .osu files with their corresponding realm beatmaps.
            newBeatmapInfo.Hash = string.Empty;
            // clear online properties.
            newBeatmapInfo.OnlineID = -1;
            newBeatmapInfo.Status = BeatmapOnlineStatus.None;

            return addDifficultyToSet(targetBeatmapSet, newBeatmap, referenceWorkingBeatmap.Skin);
        }

        private WorkingBeatmap addDifficultyToSet(BeatmapSetInfo targetBeatmapSet, IBeatmap newBeatmap, ISkin beatmapSkin)
        {
            // populate circular beatmap set info <-> beatmap info references manually.
            // several places like `BeatmapModelManager.Save()` or `GetWorkingBeatmap()`
            // rely on them being freely traversable in both directions for correct operation.
            targetBeatmapSet.Beatmaps.Add(newBeatmap.BeatmapInfo);
            newBeatmap.BeatmapInfo.BeatmapSet = targetBeatmapSet;

            beatmapModelManager.Save(newBeatmap.BeatmapInfo, newBeatmap, beatmapSkin);

            workingBeatmapCache.Invalidate(targetBeatmapSet);
            return GetWorkingBeatmap(newBeatmap.BeatmapInfo);
        }

        /// <summary>
        /// Delete a beatmap difficulty.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap difficulty to hide.</param>
        public void Hide(BeatmapInfo beatmapInfo)
        {
            realm.Run(r =>
            {
                using (var transaction = r.BeginWrite())
                {
                    if (!beatmapInfo.IsManaged)
                        beatmapInfo = r.Find<BeatmapInfo>(beatmapInfo.ID);

                    beatmapInfo.Hidden = true;
                    transaction.Commit();
                }
            });
        }

        /// <summary>
        /// Restore a beatmap difficulty.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap difficulty to restore.</param>
        public void Restore(BeatmapInfo beatmapInfo)
        {
            realm.Run(r =>
            {
                using (var transaction = r.BeginWrite())
                {
                    if (!beatmapInfo.IsManaged)
                        beatmapInfo = r.Find<BeatmapInfo>(beatmapInfo.ID);

                    beatmapInfo.Hidden = false;
                    transaction.Commit();
                }
            });
        }

        public void RestoreAll()
        {
            realm.Run(r =>
            {
                using (var transaction = r.BeginWrite())
                {
                    foreach (var beatmap in r.All<BeatmapInfo>().Where(b => b.Hidden))
                        beatmap.Hidden = false;

                    transaction.Commit();
                }
            });
        }

        /// <summary>
        /// Returns a list of all usable <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <returns>A list of available <see cref="BeatmapSetInfo"/>.</returns>
        public List<BeatmapSetInfo> GetAllUsableBeatmapSets()
        {
            return realm.Run(r =>
            {
                r.Refresh();
                return r.All<BeatmapSetInfo>().Where(b => !b.DeletePending).Detach();
            });
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public Live<BeatmapSetInfo>? QueryBeatmapSet(Expression<Func<BeatmapSetInfo, bool>> query)
        {
            return realm.Run(r => r.All<BeatmapSetInfo>().FirstOrDefault(query)?.ToLive(realm));
        }

        #region Delegation to BeatmapModelManager (methods which previously existed locally).

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public BeatmapInfo? QueryBeatmap(Expression<Func<BeatmapInfo, bool>> query) => beatmapModelManager.QueryBeatmap(query)?.Detach();

        /// <summary>
        /// Saves an <see cref="IBeatmap"/> file against a given <see cref="BeatmapInfo"/>.
        /// </summary>
        /// <param name="info">The <see cref="BeatmapInfo"/> to save the content against. The file referenced by <see cref="BeatmapInfo.Path"/> will be replaced.</param>
        /// <param name="beatmapContent">The <see cref="IBeatmap"/> content to write.</param>
        /// <param name="beatmapSkin">The beatmap <see cref="ISkin"/> content to write, null if to be omitted.</param>
        public virtual void Save(BeatmapInfo info, IBeatmap beatmapContent, ISkin? beatmapSkin = null) =>
            beatmapModelManager.Save(info, beatmapContent, beatmapSkin);

        /// <summary>
        /// A default representation of a WorkingBeatmap to use when no beatmap is available.
        /// </summary>
        public IWorkingBeatmap DefaultBeatmap => workingBeatmapCache.DefaultBeatmap;

        /// <summary>
        /// Fired when a notification should be presented to the user.
        /// </summary>
        public Action<Notification> PostNotification
        {
            set => beatmapModelManager.PostNotification = value;
        }

        #endregion

        #region Implementation of IModelManager<BeatmapSetInfo>

        public bool IsAvailableLocally(BeatmapSetInfo model)
        {
            return beatmapModelManager.IsAvailableLocally(model);
        }

        public bool Delete(BeatmapSetInfo item)
        {
            return beatmapModelManager.Delete(item);
        }

        public void Delete(List<BeatmapSetInfo> items, bool silent = false)
        {
            beatmapModelManager.Delete(items, silent);
        }

        public void Delete(Expression<Func<BeatmapSetInfo, bool>>? filter = null, bool silent = false)
        {
            realm.Run(r =>
            {
                var items = r.All<BeatmapSetInfo>().Where(s => !s.DeletePending && !s.Protected);

                if (filter != null)
                    items = items.Where(filter);

                beatmapModelManager.Delete(items.ToList(), silent);
            });
        }

        public void UndeleteAll()
        {
            realm.Run(r => beatmapModelManager.Undelete(r.All<BeatmapSetInfo>().Where(s => s.DeletePending).ToList()));
        }

        public void Undelete(List<BeatmapSetInfo> items, bool silent = false)
        {
            beatmapModelManager.Undelete(items, silent);
        }

        public void Undelete(BeatmapSetInfo item)
        {
            beatmapModelManager.Undelete(item);
        }

        #endregion

        #region Implementation of ICanAcceptFiles

        public Task Import(params string[] paths)
        {
            return beatmapModelManager.Import(paths);
        }

        public Task Import(params ImportTask[] tasks)
        {
            return beatmapModelManager.Import(tasks);
        }

        public Task<IEnumerable<Live<BeatmapSetInfo>>> Import(ProgressNotification notification, params ImportTask[] tasks)
        {
            return beatmapModelManager.Import(notification, tasks);
        }

        public Task<Live<BeatmapSetInfo>?> Import(ImportTask task, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return beatmapModelManager.Import(task, lowPriority, cancellationToken);
        }

        public Task<Live<BeatmapSetInfo>?> Import(ArchiveReader archive, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return beatmapModelManager.Import(archive, lowPriority, cancellationToken);
        }

        public Live<BeatmapSetInfo>? Import(BeatmapSetInfo item, ArchiveReader? archive = null, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return beatmapModelManager.Import(item, archive, lowPriority, cancellationToken);
        }

        public IEnumerable<string> HandledExtensions => beatmapModelManager.HandledExtensions;

        #endregion

        #region Implementation of IWorkingBeatmapCache

        public WorkingBeatmap GetWorkingBeatmap(BeatmapInfo? importedBeatmap)
        {
            // Detached sets don't come with files.
            // If we seem to be missing files, now is a good time to re-fetch.
            if (importedBeatmap?.BeatmapSet?.Files.Count == 0)
            {
                realm.Run(r =>
                {
                    var refetch = r.Find<BeatmapInfo>(importedBeatmap.ID)?.Detach();

                    if (refetch != null)
                        importedBeatmap = refetch;
                });
            }

            return workingBeatmapCache.GetWorkingBeatmap(importedBeatmap);
        }

        public WorkingBeatmap GetWorkingBeatmap(Live<BeatmapInfo>? importedBeatmap)
        {
            WorkingBeatmap working = workingBeatmapCache.GetWorkingBeatmap(null);

            importedBeatmap?.PerformRead(b => working = workingBeatmapCache.GetWorkingBeatmap(b));

            return working;
        }

        void IWorkingBeatmapCache.Invalidate(BeatmapSetInfo beatmapSetInfo) => workingBeatmapCache.Invalidate(beatmapSetInfo);
        void IWorkingBeatmapCache.Invalidate(BeatmapInfo beatmapInfo) => workingBeatmapCache.Invalidate(beatmapInfo);

        #endregion

        #region Implementation of IModelFileManager<in BeatmapSetInfo,in BeatmapSetFileInfo>

        public void ReplaceFile(BeatmapSetInfo model, RealmNamedFileUsage file, Stream contents)
        {
            beatmapModelManager.ReplaceFile(model, file, contents);
        }

        public void DeleteFile(BeatmapSetInfo model, RealmNamedFileUsage file)
        {
            beatmapModelManager.DeleteFile(model, file);
        }

        public void AddFile(BeatmapSetInfo model, Stream contents, string filename)
        {
            beatmapModelManager.AddFile(model, contents, filename);
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            onlineBeatmapLookupQueue?.Dispose();
        }

        #endregion

        #region Implementation of IPostImports<out BeatmapSetInfo>

        public Action<IEnumerable<Live<BeatmapSetInfo>>>? PostImport
        {
            set => beatmapModelManager.PostImport = value;
        }

        #endregion
    }
}
