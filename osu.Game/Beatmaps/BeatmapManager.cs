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

            var beatmapSet = new BeatmapSetInfo
            {
                Beatmaps =
                {
                    new BeatmapInfo
                    {
                        Difficulty = new BeatmapDifficulty(),
                        Ruleset = ruleset,
                        Metadata = metadata,
                        WidescreenStoryboard = true,
                        SamplesMatchPlaybackRate = true,
                    }
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
