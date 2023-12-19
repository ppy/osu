// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Models;
using osu.Game.Overlays.Notifications;
using Realms;

namespace osu.Game.Database
{
    public class ModelManager<TModel> : IModelManager<TModel>, IModelFileManager<TModel, RealmNamedFileUsage>
        where TModel : RealmObject, IHasRealmFiles, IHasGuidPrimaryKey, ISoftDelete
    {
        /// <summary>
        /// Temporarily pause imports to avoid performance overheads affecting gameplay scenarios.
        /// </summary>
        public virtual bool PauseImports { get; set; }

        protected RealmAccess Realm { get; }

        private readonly RealmFileStore realmFileStore;

        public ModelManager(Storage storage, RealmAccess realm)
        {
            realmFileStore = new RealmFileStore(realm, storage);
            Realm = realm;
        }

        public void DeleteFile(TModel item, RealmNamedFileUsage file) =>
            performFileOperation(item, managed => DeleteFile(managed, managed.Files.First(f => f.Filename == file.Filename), managed.Realm!));

        public void ReplaceFile(TModel item, RealmNamedFileUsage file, Stream contents) =>
            performFileOperation(item, managed => ReplaceFile(file, contents, managed.Realm!));

        public void AddFile(TModel item, Stream contents, string filename) =>
            performFileOperation(item, managed => AddFile(managed, contents, filename, managed.Realm!));

        private void performFileOperation(TModel item, Action<TModel> operation)
        {
            // While we are detaching so often, this seems like the easiest way to keep things in sync.
            // This method should be removed as soon as all the surrounding pieces support non-detached operations.
            if (!item.IsManaged)
            {
                // Importantly, begin the realm write *before* re-fetching, else the update realm may not be in a consistent state
                // (ie. if an async import finished very recently).
                Realm.Realm.Write(realm =>
                {
                    var managed = realm.FindWithRefresh<TModel>(item.ID);
                    Debug.Assert(managed != null);
                    operation(managed);

                    item.Files.Clear();
                    item.Files.AddRange(managed.Files.Detach());
                });
            }
            else
                operation(item);
        }

        /// <summary>
        /// Delete a file from within an ongoing realm transaction.
        /// </summary>
        public void DeleteFile(TModel item, RealmNamedFileUsage file, Realm realm)
        {
            item.Files.Remove(file);
        }

        /// <summary>
        /// Replace a file from within an ongoing realm transaction.
        /// </summary>
        public void ReplaceFile(RealmNamedFileUsage file, Stream contents, Realm realm)
        {
            file.File = realmFileStore.Add(contents, realm);
        }

        /// <summary>
        /// Add a file from within an ongoing realm transaction. If the file already exists, it is overwritten.
        /// </summary>
        public void AddFile(TModel item, Stream contents, string filename, Realm realm)
        {
            var existing = item.GetFile(filename);

            if (existing != null)
            {
                ReplaceFile(existing, contents, realm);
                return;
            }

            var file = realmFileStore.Add(contents, realm);
            var namedUsage = new RealmNamedFileUsage(file, filename);

            item.Files.Add(namedUsage);
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
            // Importantly, begin the realm write *before* re-fetching, else the update realm may not be in a consistent state
            // (ie. if an async import finished very recently).
            return Realm.Write(realm =>
            {
                TModel? processableItem = item;
                if (!processableItem.IsManaged)
                    processableItem = realm.Find<TModel>(item.ID);

                if (processableItem?.DeletePending != false)
                    return false;

                processableItem.DeletePending = true;
                return true;
            });
        }

        public void Undelete(TModel item)
        {
            // Importantly, begin the realm write *before* re-fetching, else the update realm may not be in a consistent state
            // (ie. if an async import finished very recently).
            Realm.Write(realm =>
            {
                TModel? processableItem = item;
                if (!processableItem.IsManaged)
                    processableItem = realm.Find<TModel>(item.ID);

                if (processableItem?.DeletePending != true)
                    return;

                processableItem.DeletePending = false;
            });
        }

        public virtual bool IsAvailableLocally(TModel model) => true;

        public Action<Notification>? PostNotification { get; set; }

        public virtual string HumanisedModelName => $"{typeof(TModel).Name.Replace(@"Info", "").ToLowerInvariant()}";
    }
}
