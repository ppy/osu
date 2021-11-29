// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Models;
using osu.Game.Overlays.Notifications;
using Realms;

#nullable enable

namespace osu.Game.Stores
{
    /// <summary>
    /// Class which adds all the missing pieces bridging the gap between <see cref="RealmArchiveModelImporter{TModel}"/> and <see cref="ArchiveModelManager{TModel,TFileModel}"/>.
    /// </summary>
    public abstract class RealmArchiveModelManager<TModel> : RealmArchiveModelImporter<TModel>, IModelManager<TModel>, IModelFileManager<TModel, RealmNamedFileUsage>
        where TModel : RealmObject, IHasRealmFiles, IHasGuidPrimaryKey, ISoftDelete
    {
        public event Action<TModel>? ItemUpdated;
        public event Action<TModel>? ItemRemoved;

        private readonly RealmFileStore realmFileStore;

        protected RealmArchiveModelManager(Storage storage, RealmContextFactory contextFactory)
            : base(storage, contextFactory)
        {
            realmFileStore = new RealmFileStore(contextFactory, storage);
        }

        public void DeleteFile(TModel item, RealmNamedFileUsage file) =>
            item.Realm.Write(() => DeleteFile(item, file, item.Realm));

        public void ReplaceFile(TModel item, RealmNamedFileUsage file, Stream contents)
            => item.Realm.Write(() => ReplaceFile(item, file, contents, item.Realm));

        public void AddFile(TModel item, Stream stream, string filename)
            => item.Realm.Write(() => AddFile(item, stream, filename, item.Realm));

        public void DeleteFile(TModel item, RealmNamedFileUsage file, Realm realm)
        {
            item.Files.Remove(file);
        }

        public void ReplaceFile(TModel model, RealmNamedFileUsage file, Stream contents, Realm realm)
        {
            file.File = realmFileStore.Add(contents, realm);
        }

        public void AddFile(TModel item, Stream stream, string filename, Realm realm)
        {
            var file = realmFileStore.Add(stream, realm);
            var namedUsage = new RealmNamedFileUsage(file, filename);

            item.Files.Add(namedUsage);
        }

        public override async Task<ILive<TModel>?> Import(TModel item, ArchiveReader? archive = null, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            var imported = await base.Import(item, archive, lowPriority, cancellationToken).ConfigureAwait(false);

            imported?.PerformRead(i => ItemUpdated?.Invoke(i.Detach()));

            return imported;
        }

        /// <summary>
        /// Delete multiple items.
        /// This will post notifications tracking progress.
        /// </summary>
        public void Delete(List<TModel> items, bool silent = false)
        {
            if (items.Count == 0) return;

            var notification = new ProgressNotification
            {
                Progress = 0,
                Text = $"Preparing to delete all {HumanisedModelName}s...",
                CompletionText = $"Deleted all {HumanisedModelName}s!",
                State = ProgressNotificationState.Active,
            };

            if (!silent)
                PostNotification?.Invoke(notification);

            int i = 0;

            foreach (var b in items)
            {
                if (notification.State == ProgressNotificationState.Cancelled)
                    // user requested abort
                    return;

                notification.Text = $"Deleting {HumanisedModelName}s ({++i} of {items.Count})";

                Delete(b);

                notification.Progress = (float)i / items.Count;
            }

            notification.State = ProgressNotificationState.Completed;
        }

        /// <summary>
        /// Restore multiple items that were previously deleted.
        /// This will post notifications tracking progress.
        /// </summary>
        public void Undelete(List<TModel> items, bool silent = false)
        {
            if (!items.Any()) return;

            var notification = new ProgressNotification
            {
                CompletionText = "Restored all deleted items!",
                Progress = 0,
                State = ProgressNotificationState.Active,
            };

            if (!silent)
                PostNotification?.Invoke(notification);

            int i = 0;

            foreach (var item in items)
            {
                if (notification.State == ProgressNotificationState.Cancelled)
                    // user requested abort
                    return;

                notification.Text = $"Restoring ({++i} of {items.Count})";

                Undelete(item);

                notification.Progress = (float)i / items.Count;
            }

            notification.State = ProgressNotificationState.Completed;
        }

        public bool Delete(TModel item)
        {
            if (item.DeletePending)
                return false;

            item.Realm.Write(r => item.DeletePending = true);
            ItemRemoved?.Invoke(item.Detach());
            return true;
        }

        public void Undelete(TModel item)
        {
            if (!item.DeletePending)
                return;

            item.Realm.Write(r => item.DeletePending = false);
            ItemUpdated?.Invoke(item);
        }

        public virtual bool IsAvailableLocally(TModel model) => false; // TODO: implement

        public void Update(TModel skin)
        {
        }
    }
}
