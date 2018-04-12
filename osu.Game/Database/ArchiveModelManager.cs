// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.IPC;
using osu.Game.Overlays.Notifications;
using osu.Game.Utils;
using SharpCompress.Common;
using FileInfo = osu.Game.IO.FileInfo;

namespace osu.Game.Database
{
    /// <summary>
    /// Encapsulates a model store class to give it import functionality.
    /// Adds cross-functionality with <see cref="FileStore"/> to give access to the central file store for the provided model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TFileModel">The associated file join type.</typeparam>
    public abstract class ArchiveModelManager<TModel, TFileModel> : ICanAcceptFiles
        where TModel : class, IHasFiles<TFileModel>, IHasPrimaryKey, ISoftDelete
        where TFileModel : INamedFileInfo, new()
    {
        /// <summary>
        /// Set an endpoint for notifications to be posted to.
        /// </summary>
        public Action<Notification> PostNotification { protected get; set; }

        /// <summary>
        /// Fired when a new <see cref="TModel"/> becomes available in the database.
        /// This is not guaranteed to run on the update thread.
        /// </summary>
        public event Action<TModel> ItemAdded;

        /// <summary>
        /// Fired when a <see cref="TModel"/> is removed from the database.
        /// This is not guaranteed to run on the update thread.
        /// </summary>
        public event Action<TModel> ItemRemoved;

        public virtual string[] HandledExtensions => new[] { ".zip" };

        protected readonly FileStore Files;

        protected readonly IDatabaseContextFactory ContextFactory;

        protected readonly MutableDatabaseBackedStore<TModel> ModelStore;

        // ReSharper disable once NotAccessedField.Local (we should keep a reference to this so it is not finalised)
        private ArchiveImportIPCChannel ipc;

        protected ArchiveModelManager(Storage storage, IDatabaseContextFactory contextFactory, MutableDatabaseBackedStore<TModel> modelStore, IIpcHost importHost = null)
        {
            ContextFactory = contextFactory;

            ModelStore = modelStore;
            ModelStore.ItemAdded += s => ItemAdded?.Invoke(s);
            ModelStore.ItemRemoved += s => ItemRemoved?.Invoke(s);

            Files = new FileStore(contextFactory, storage);

            if (importHost != null)
                ipc = new ArchiveImportIPCChannel(importHost, this);

            ModelStore.Cleanup();
        }

        /// <summary>
        /// Import one or more <see cref="TModel"/> items from filesystem <paramref name="paths"/>.
        /// This will post notifications tracking progress.
        /// </summary>
        /// <param name="paths">One or more archive locations on disk.</param>
        public void Import(params string[] paths)
        {
            var notification = new ProgressNotification
            {
                Text = "Import is initialising...",
                Progress = 0,
                State = ProgressNotificationState.Active,
            };

            PostNotification?.Invoke(notification);

            List<TModel> imported = new List<TModel>();

            int current = 0;
            int errors = 0;
            foreach (string path in paths)
            {
                if (notification.State == ProgressNotificationState.Cancelled)
                    // user requested abort
                    return;

                try
                {
                    notification.Text = $"Importing ({++current} of {paths.Length})\n{Path.GetFileName(path)}";
                    using (ArchiveReader reader = getReaderFrom(path))
                        imported.Add(Import(reader));

                    notification.Progress = (float)current / paths.Length;

                    // We may or may not want to delete the file depending on where it is stored.
                    //  e.g. reconstructing/repairing database with items from default storage.
                    // Also, not always a single file, i.e. for LegacyFilesystemReader
                    // TODO: Add a check to prevent files from storage to be deleted.
                    try
                    {
                        if (File.Exists(path))
                            File.Delete(path);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $@"Could not delete original file after import ({Path.GetFileName(path)})");
                    }
                }
                catch (Exception e)
                {
                    e = e.InnerException ?? e;
                    Logger.Error(e, $@"Could not import ({Path.GetFileName(path)})");
                    errors++;
                }
            }

            notification.Text = errors > 0 ? $"Import complete with {errors} errors" : "Import successful!";
            notification.State = ProgressNotificationState.Completed;
        }

        /// <summary>
        /// Import an item from an <see cref="ArchiveReader"/>.
        /// </summary>
        /// <param name="archive">The archive to be imported.</param>
        public TModel Import(ArchiveReader archive)
        {
            using (ContextFactory.GetForWrite()) // used to share a context for full import. keep in mind this will block all writes.
            {
                // create a new model (don't yet add to database)
                var item = CreateModel(archive);

                var existing = CheckForExisting(item);

                if (existing != null) return existing;

                item.Files = createFileInfos(archive, Files);

                Populate(item, archive);

                // import to store
                ModelStore.Add(item);

                return item;
            }
        }

        /// <summary>
        /// Import an item from a <see cref="TModel"/>.
        /// </summary>
        /// <param name="item">The model to be imported.</param>
        public void Import(TModel item) => ModelStore.Add(item);

        /// <summary>
        /// Perform an update of the specified item.
        /// TODO: Support file changes.
        /// </summary>
        /// <param name="item">The item to update.</param>
        public void Update(TModel item) => ModelStore.Update(item);

        /// <summary>
        /// Delete an item from the manager.
        /// Is a no-op for already deleted items.
        /// </summary>
        /// <param name="item">The item to delete.</param>
        public void Delete(TModel item)
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                var context = usage.Context;

                context.ChangeTracker.AutoDetectChangesEnabled = false;

                // re-fetch the model on the import context.
                var foundModel = queryModel().Include(s => s.Files).ThenInclude(f => f.FileInfo).First(s => s.ID == item.ID);

                if (foundModel.DeletePending) return;

                if (ModelStore.Delete(foundModel))
                    Files.Dereference(foundModel.Files.Select(f => f.FileInfo).ToArray());

                context.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        /// <summary>
        /// Delete multiple items.
        /// This will post notifications tracking progress.
        /// </summary>
        public void Delete(List<TModel> items)
        {
            if (items.Count == 0) return;

            var notification = new ProgressNotification
            {
                Progress = 0,
                CompletionText = "Deleted all beatmaps!",
                State = ProgressNotificationState.Active,
            };

            PostNotification?.Invoke(notification);

            int i = 0;

            using (ContextFactory.GetForWrite())
            {
                foreach (var b in items)
                {
                    if (notification.State == ProgressNotificationState.Cancelled)
                        // user requested abort
                        return;

                    notification.Text = $"Deleting ({++i} of {items.Count})";

                    Delete(b);

                    notification.Progress = (float)i / items.Count;
                }
            }

            notification.State = ProgressNotificationState.Completed;
        }

        /// <summary>
        /// Restore multiple items that were previously deleted.
        /// This will post notifications tracking progress.
        /// </summary>
        public void Undelete(List<TModel> items)
        {
            if (!items.Any()) return;

            var notification = new ProgressNotification
            {
                CompletionText = "Restored all deleted items!",
                Progress = 0,
                State = ProgressNotificationState.Active,
            };

            PostNotification?.Invoke(notification);

            int i = 0;

            using (ContextFactory.GetForWrite())
            {
                foreach (var item in items)
                {
                    if (notification.State == ProgressNotificationState.Cancelled)
                        // user requested abort
                        return;

                    notification.Text = $"Restoring ({++i} of {items.Count})";

                    Undelete(item);

                    notification.Progress = (float)i / items.Count;
                }
            }

            notification.State = ProgressNotificationState.Completed;
        }

        /// <summary>
        /// Restore an item that was previously deleted. Is a no-op if the item is not in a deleted state, or has its protected flag set.
        /// </summary>
        /// <param name="item">The item to restore</param>
        public void Undelete(TModel item)
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                usage.Context.ChangeTracker.AutoDetectChangesEnabled = false;

                if (!ModelStore.Undelete(item)) return;

                Files.Reference(item.Files.Select(f => f.FileInfo).ToArray());

                usage.Context.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        /// <summary>
        /// Create all required <see cref="FileInfo"/>s for the provided archive, adding them to the global file store.
        /// </summary>
        private List<TFileModel> createFileInfos(ArchiveReader reader, FileStore files)
        {
            var fileInfos = new List<TFileModel>();

            // import files to manager
            foreach (string file in reader.Filenames)
                using (Stream s = reader.GetStream(file))
                    fileInfos.Add(new TFileModel
                    {
                        Filename = file,
                        FileInfo = files.Add(s)
                    });

            return fileInfos;
        }

        /// <summary>
        /// Create a barebones model from the provided archive.
        /// Actual expensive population should be done in <see cref="Populate"/>; this should just prepare for duplicate checking.
        /// </summary>
        /// <param name="archive">The archive to create the model for.</param>
        /// <returns>A model populated with minimal information.</returns>
        protected abstract TModel CreateModel(ArchiveReader archive);

        /// <summary>
        /// Populate the provided model completely from the given archive.
        /// After this method, the model should be in a state ready to commit to a store.
        /// </summary>
        /// <param name="model">The model to populate.</param>
        /// <param name="archive">The archive to use as a reference for population.</param>
        protected virtual void Populate(TModel model, ArchiveReader archive)
        {
        }

        protected virtual TModel CheckForExisting(TModel model) => null;

        private DbSet<TModel> queryModel() => ContextFactory.Get().Set<TModel>();

        /// <summary>
        /// Creates an <see cref="ArchiveReader"/> from a valid storage path.
        /// </summary>
        /// <param name="path">A file or folder path resolving the archive content.</param>
        /// <returns>A reader giving access to the archive's content.</returns>
        private ArchiveReader getReaderFrom(string path)
        {
            if (ZipUtils.IsZipArchive(path))
                return new ZipArchiveReader(Files.Storage.GetStream(path), Path.GetFileName(path));
            if (Directory.Exists(path))
                return new LegacyFilesystemReader(path);
            throw new InvalidFormatException($"{path} is not a valid archive");
        }
    }
}
