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
using osu.Framework.Extensions;
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

        private readonly RealmContextFactory contextFactory;

        public BeatmapManager(Storage storage, RealmContextFactory contextFactory, RulesetStore rulesets, IAPIProvider api, AudioManager audioManager, IResourceStore<byte[]> gameResources, GameHost? host = null, WorkingBeatmap? defaultBeatmap = null, bool performOnlineLookups = false)
        {
            this.contextFactory = contextFactory;
            if (performOnlineLookups)
                onlineBeatmapLookupQueue = new BeatmapOnlineLookupQueue(api, storage);

            var userResources = new RealmFileStore(contextFactory, storage).Store;

            BeatmapTrackStore = audioManager.GetTrackStore(userResources);

            beatmapModelManager = CreateBeatmapModelManager(storage, contextFactory, rulesets, onlineBeatmapLookupQueue);
            workingBeatmapCache = CreateWorkingBeatmapCache(audioManager, gameResources, userResources, defaultBeatmap, host);

            workingBeatmapCache.BeatmapManager = beatmapModelManager;
            beatmapModelManager.WorkingBeatmapCache = workingBeatmapCache;
        }

        protected virtual WorkingBeatmapCache CreateWorkingBeatmapCache(AudioManager audioManager, IResourceStore<byte[]> resources, IResourceStore<byte[]> storage, WorkingBeatmap? defaultBeatmap, GameHost? host)
        {
            return new WorkingBeatmapCache(BeatmapTrackStore, audioManager, resources, storage, defaultBeatmap, host);
        }

        protected virtual BeatmapModelManager CreateBeatmapModelManager(Storage storage, RealmContextFactory contextFactory, RulesetStore rulesets, BeatmapOnlineLookupQueue? onlineLookupQueue) =>
            new BeatmapModelManager(contextFactory, storage, onlineLookupQueue);

        /// <summary>
        /// Create a new <see cref="WorkingBeatmap"/>.
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

            var set = new BeatmapSetInfo
            {
                Beatmaps =
                {
                    new BeatmapInfo
                    {
                        BaseDifficulty = new BeatmapDifficulty(),
                        Ruleset = ruleset,
                        Metadata = metadata,
                        WidescreenStoryboard = true,
                        SamplesMatchPlaybackRate = true,
                    }
                }
            };

            var imported = beatmapModelManager.Import(set).GetResultSafely()?.Value;

            return GetWorkingBeatmap(imported?.Beatmaps.First());
        }

        #region Delegation to BeatmapModelManager (methods which previously existed locally).

        /// <summary>
        /// Fired when a single difficulty has been hidden.
        /// </summary>
        public event Action<BeatmapInfo>? BeatmapHidden;

        /// <summary>
        /// Fired when a single difficulty has been restored.
        /// </summary>
        public event Action<BeatmapInfo>? BeatmapRestored;

        /// <summary>
        /// Delete a beatmap difficulty.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap difficulty to hide.</param>
        public void Hide(BeatmapInfo beatmapInfo)
        {
            using (var realm = contextFactory.CreateContext())
            using (var transaction = realm.BeginWrite())
            {
                beatmapInfo.Hidden = true;
                transaction.Commit();

                BeatmapHidden?.Invoke(beatmapInfo);
            }
        }

        /// <summary>
        /// Restore a beatmap difficulty.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap difficulty to restore.</param>
        public void Restore(BeatmapInfo beatmapInfo)
        {
            using (var realm = contextFactory.CreateContext())
            using (var transaction = realm.BeginWrite())
            {
                beatmapInfo.Hidden = false;
                transaction.Commit();

                BeatmapRestored?.Invoke(beatmapInfo);
            }
        }

        /// <summary>
        /// Saves an <see cref="IBeatmap"/> file against a given <see cref="BeatmapInfo"/>.
        /// </summary>
        /// <param name="info">The <see cref="BeatmapInfo"/> to save the content against. The file referenced by <see cref="BeatmapInfo.Path"/> will be replaced.</param>
        /// <param name="beatmapContent">The <see cref="IBeatmap"/> content to write.</param>
        /// <param name="beatmapSkin">The beatmap <see cref="ISkin"/> content to write, null if to be omitted.</param>
        public virtual void Save(BeatmapInfo info, IBeatmap beatmapContent, ISkin? beatmapSkin = null) =>
            beatmapModelManager.Save(info, beatmapContent, beatmapSkin);

        /// <summary>
        /// Returns a list of all usable <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <returns>A list of available <see cref="BeatmapSetInfo"/>.</returns>
        public List<BeatmapSetInfo> GetAllUsableBeatmapSets(bool includeProtected = false) => beatmapModelManager.GetAllUsableBeatmapSets(includeProtected);

        /// <summary>
        /// Returns a list of all usable <see cref="BeatmapSetInfo"/>s. Note that files are not populated.
        /// </summary>
        /// <param name="includeProtected">Whether to include protected (system) beatmaps. These should not be included for gameplay playable use cases.</param>
        /// <returns>A list of available <see cref="BeatmapSetInfo"/>.</returns>
        public IEnumerable<BeatmapSetInfo> GetAllUsableBeatmapSetsEnumerable(bool includeProtected = false) => beatmapModelManager.GetAllUsableBeatmapSetsEnumerable(includeProtected);

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Results from the provided query.</returns>
        public ILive<IEnumerable<BeatmapSetInfo>> QueryBeatmapSets(Expression<Func<BeatmapSetInfo, bool>> query) => beatmapModelManager.QueryBeatmapSets(query).ToLive(contextFactory);

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public BeatmapSetInfo? QueryBeatmapSet(Expression<Func<BeatmapSetInfo, bool>> query) => beatmapModelManager.QueryBeatmapSet(query);

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Results from the provided query.</returns>
        public IQueryable<BeatmapInfo> QueryBeatmaps(Expression<Func<BeatmapInfo, bool>> query) => beatmapModelManager.QueryBeatmaps(query);

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public BeatmapInfo? QueryBeatmap(Expression<Func<BeatmapInfo, bool>> query) => beatmapModelManager.QueryBeatmap(query);

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

        public event Action<BeatmapSetInfo>? ItemUpdated;

        public event Action<BeatmapSetInfo>? ItemRemoved;

        public void Update(BeatmapSetInfo item)
        {
            beatmapModelManager.Update(item);
        }

        public bool Delete(BeatmapSetInfo item)
        {
            return beatmapModelManager.Delete(item);
        }

        public void Delete(List<BeatmapSetInfo> items, bool silent = false)
        {
            beatmapModelManager.Delete(items, silent);
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

        public Task<IEnumerable<ILive<BeatmapSetInfo>>> Import(ProgressNotification notification, params ImportTask[] tasks)
        {
            return beatmapModelManager.Import(notification, tasks);
        }

        public Task<ILive<BeatmapSetInfo>?> Import(ImportTask task, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return beatmapModelManager.Import(task, lowPriority, cancellationToken);
        }

        public Task<ILive<BeatmapSetInfo>?> Import(ArchiveReader archive, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return beatmapModelManager.Import(archive, lowPriority, cancellationToken);
        }

        public Task<ILive<BeatmapSetInfo>?> Import(BeatmapSetInfo item, ArchiveReader? archive = null, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return beatmapModelManager.Import(item, archive, lowPriority, cancellationToken);
        }

        public IEnumerable<string> HandledExtensions => beatmapModelManager.HandledExtensions;

        #endregion

        #region Implementation of IWorkingBeatmapCache

        public WorkingBeatmap GetWorkingBeatmap(BeatmapInfo? importedBeatmap) => workingBeatmapCache.GetWorkingBeatmap(importedBeatmap);

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

        public Action<IEnumerable<ILive<BeatmapSetInfo>>>? PostImport
        {
            set => beatmapModelManager.PostImport = value;
        }

        #endregion
    }
}
