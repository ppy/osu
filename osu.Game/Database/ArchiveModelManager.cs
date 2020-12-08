// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using osu.Framework;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.IPC;
using osu.Game.Overlays.Notifications;
using SharpCompress.Archives.Zip;
using FileInfo = osu.Game.IO.FileInfo;

namespace osu.Game.Database
{
    /// <summary>
    /// Encapsulates a model store class to give it import functionality.
    /// Adds cross-functionality with <see cref="FileStore"/> to give access to the central file store for the provided model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TFileModel">The associated file join type.</typeparam>
    public abstract class ArchiveModelManager<TModel, TFileModel> : ICanAcceptFiles, IModelManager<TModel>
        where TModel : class, IHasFiles<TFileModel>, IHasPrimaryKey, ISoftDelete
        where TFileModel : class, INamedFileInfo, new()
    {
        private const int import_queue_request_concurrency = 1;

        /// <summary>
        /// A singleton scheduler shared by all <see cref="ArchiveModelManager{TModel,TFileModel}"/>.
        /// </summary>
        /// <remarks>
        /// This scheduler generally performs IO and CPU intensive work so concurrency is limited harshly.
        /// It is mainly being used as a queue mechanism for large imports.
        /// </remarks>
        private static readonly ThreadedTaskScheduler import_scheduler = new ThreadedTaskScheduler(import_queue_request_concurrency, nameof(ArchiveModelManager<TModel, TFileModel>));

        /// <summary>
        /// Set an endpoint for notifications to be posted to.
        /// </summary>
        public Action<Notification> PostNotification { protected get; set; }

        /// <summary>
        /// Fired when a new or updated <typeparamref name="TModel"/> becomes available in the database.
        /// This is not guaranteed to run on the update thread.
        /// </summary>
        public IBindable<WeakReference<TModel>> ItemUpdated => itemUpdated;

        private readonly Bindable<WeakReference<TModel>> itemUpdated = new Bindable<WeakReference<TModel>>();

        /// <summary>
        /// Fired when a <typeparamref name="TModel"/> is removed from the database.
        /// This is not guaranteed to run on the update thread.
        /// </summary>
        public IBindable<WeakReference<TModel>> ItemRemoved => itemRemoved;

        private readonly Bindable<WeakReference<TModel>> itemRemoved = new Bindable<WeakReference<TModel>>();

        public virtual IEnumerable<string> HandledExtensions => new[] { ".zip" };

        public virtual bool SupportsImportFromStable => RuntimeInfo.IsDesktop;

        protected readonly FileStore Files;

        protected readonly IDatabaseContextFactory ContextFactory;

        protected readonly MutableDatabaseBackedStore<TModel> ModelStore;

        // ReSharper disable once NotAccessedField.Local (we should keep a reference to this so it is not finalised)
        private ArchiveImportIPCChannel ipc;

        private readonly Storage exportStorage;

        protected ArchiveModelManager(Storage storage, IDatabaseContextFactory contextFactory, MutableDatabaseBackedStoreWithFileIncludes<TModel, TFileModel> modelStore, IIpcHost importHost = null)
        {
            ContextFactory = contextFactory;

            ModelStore = modelStore;
            ModelStore.ItemUpdated += item => handleEvent(() => itemUpdated.Value = new WeakReference<TModel>(item));
            ModelStore.ItemRemoved += item => handleEvent(() => itemRemoved.Value = new WeakReference<TModel>(item));

            exportStorage = storage.GetStorageForDirectory("exports");

            Files = new FileStore(contextFactory, storage);

            if (importHost != null)
                ipc = new ArchiveImportIPCChannel(importHost, this);

            ModelStore.Cleanup();
        }

        /// <summary>
        /// Import one or more <typeparamref name="TModel"/> items from filesystem <paramref name="paths"/>.
        /// This will post notifications tracking progress.
        /// </summary>
        /// <param name="paths">One or more archive locations on disk.</param>
        public Task Import(params string[] paths)
        {
            var notification = new ProgressNotification { State = ProgressNotificationState.Active };

            PostNotification?.Invoke(notification);

            return Import(notification, paths.Select(p => new ImportTask(p)).ToArray());
        }

        public Task Import(Stream stream, string filename)
        {
            var notification = new ProgressNotification { State = ProgressNotificationState.Active };

            PostNotification?.Invoke(notification);

            return Import(notification, new ImportTask(stream, filename));
        }

        protected async Task<IEnumerable<TModel>> Import(ProgressNotification notification, params ImportTask[] tasks)
        {
            notification.Progress = 0;
            notification.Text = $"{HumanisedModelName.Humanize(LetterCasing.Title)} import is initialising...";

            int current = 0;

            var imported = new List<TModel>();

            await Task.WhenAll(tasks.Select(async task =>
            {
                notification.CancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var model = await Import(task, notification.CancellationToken);

                    lock (imported)
                    {
                        if (model != null)
                            imported.Add(model);
                        current++;

                        notification.Text = $"Imported {current} of {tasks.Length} {HumanisedModelName}s";
                        notification.Progress = (float)current / tasks.Length;
                    }
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.Error(e, $@"Could not import ({task})", LoggingTarget.Database);
                }
            }));

            if (imported.Count == 0)
            {
                notification.Text = $"{HumanisedModelName.Humanize(LetterCasing.Title)} import failed!";
                notification.State = ProgressNotificationState.Cancelled;
            }
            else
            {
                notification.CompletionText = imported.Count == 1
                    ? $"Imported {imported.First()}!"
                    : $"Imported {imported.Count} {HumanisedModelName}s!";

                if (imported.Count > 0 && PresentImport != null)
                {
                    notification.CompletionText += " Click to view.";
                    notification.CompletionClickAction = () =>
                    {
                        PresentImport?.Invoke(imported);
                        return true;
                    };
                }

                notification.State = ProgressNotificationState.Completed;
            }

            return imported;
        }

        /// <summary>
        /// Import one <typeparamref name="TModel"/> from the filesystem and delete the file on success.
        /// Note that this bypasses the UI flow and should only be used for special cases or testing.
        /// </summary>
        /// <param name="task">The <see cref="ImportTask"/> containing data about the <typeparamref name="TModel"/> to import.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The imported model, if successful.</returns>
        internal async Task<TModel> Import(ImportTask task, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TModel import;
            using (ArchiveReader reader = task.GetReader())
                import = await Import(reader, cancellationToken);

            // We may or may not want to delete the file depending on where it is stored.
            //  e.g. reconstructing/repairing database with items from default storage.
            // Also, not always a single file, i.e. for LegacyFilesystemReader
            // TODO: Add a check to prevent files from storage to be deleted.
            try
            {
                if (import != null && File.Exists(task.Path) && ShouldDeleteArchive(task.Path))
                    File.Delete(task.Path);
            }
            catch (Exception e)
            {
                LogForModel(import, $@"Could not delete original file after import ({task})", e);
            }

            return import;
        }

        /// <summary>
        /// Fired when the user requests to view the resulting import.
        /// </summary>
        public Action<IEnumerable<TModel>> PresentImport;

        /// <summary>
        /// Import an item from an <see cref="ArchiveReader"/>.
        /// </summary>
        /// <param name="archive">The archive to be imported.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        public Task<TModel> Import(ArchiveReader archive, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TModel model = null;

            try
            {
                model = CreateModel(archive);

                if (model == null)
                    return Task.FromResult<TModel>(null);
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                LogForModel(model, $"Model creation of {archive.Name} failed.", e);
                return null;
            }

            return Import(model, archive, cancellationToken);
        }

        /// <summary>
        /// Any file extensions which should be included in hash creation.
        /// Generally should include all file types which determine the file's uniqueness.
        /// Large files should be avoided if possible.
        /// </summary>
        /// <remarks>
        /// This is only used by the default hash implementation. If <see cref="ComputeHash"/> is overridden, it will not be used.
        /// </remarks>
        protected abstract string[] HashableFileTypes { get; }

        internal static void LogForModel(TModel model, string message, Exception e = null)
        {
            string prefix = $"[{(model?.Hash ?? "?????").Substring(0, 5)}]";

            if (e != null)
                Logger.Error(e, $"{prefix} {message}", LoggingTarget.Database);
            else
                Logger.Log($"{prefix} {message}", LoggingTarget.Database);
        }

        /// <summary>
        /// Create a SHA-2 hash from the provided archive based on file content of all files matching <see cref="HashableFileTypes"/>.
        /// </summary>
        /// <remarks>
        ///  In the case of no matching files, a hash will be generated from the passed archive's <see cref="ArchiveReader.Name"/>.
        /// </remarks>
        protected virtual string ComputeHash(TModel item, ArchiveReader reader = null)
        {
            // for now, concatenate all .osu files in the set to create a unique hash.
            MemoryStream hashable = new MemoryStream();

            foreach (TFileModel file in item.Files.Where(f => HashableFileTypes.Any(ext => f.Filename.EndsWith(ext, StringComparison.OrdinalIgnoreCase))).OrderBy(f => f.Filename))
            {
                using (Stream s = Files.Store.GetStream(file.FileInfo.StoragePath))
                    s.CopyTo(hashable);
            }

            if (hashable.Length > 0)
                return hashable.ComputeSHA2Hash();

            if (reader != null)
                return reader.Name.ComputeSHA2Hash();

            return item.Hash;
        }

        /// <summary>
        /// Import an item from a <typeparamref name="TModel"/>.
        /// </summary>
        /// <param name="item">The model to be imported.</param>
        /// <param name="archive">An optional archive to use for model population.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        public async Task<TModel> Import(TModel item, ArchiveReader archive = null, CancellationToken cancellationToken = default) => await Task.Factory.StartNew(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            delayEvents();

            void rollback()
            {
                if (!Delete(item))
                {
                    // We may have not yet added the model to the underlying table, but should still clean up files.
                    LogForModel(item, "Dereferencing files for incomplete import.");
                    Files.Dereference(item.Files.Select(f => f.FileInfo).ToArray());
                }
            }

            try
            {
                LogForModel(item, "Beginning import...");

                item.Files = archive != null ? createFileInfos(archive, Files) : new List<TFileModel>();
                item.Hash = ComputeHash(item, archive);

                await Populate(item, archive, cancellationToken);

                using (var write = ContextFactory.GetForWrite()) // used to share a context for full import. keep in mind this will block all writes.
                {
                    try
                    {
                        if (!write.IsTransactionLeader) throw new InvalidOperationException($"Ensure there is no parent transaction so errors can correctly be handled by {this}");

                        var existing = CheckForExisting(item);

                        if (existing != null)
                        {
                            if (CanReuseExisting(existing, item))
                            {
                                Undelete(existing);
                                LogForModel(item, $"Found existing {HumanisedModelName} for {item} (ID {existing.ID}) – skipping import.");
                                // existing item will be used; rollback new import and exit early.
                                rollback();
                                flushEvents(true);
                                return existing;
                            }

                            Delete(existing);
                            ModelStore.PurgeDeletable(s => s.ID == existing.ID);
                        }

                        PreImport(item);

                        // import to store
                        ModelStore.Add(item);
                    }
                    catch (Exception e)
                    {
                        write.Errors.Add(e);
                        throw;
                    }
                }

                LogForModel(item, "Import successfully completed!");
            }
            catch (Exception e)
            {
                if (!(e is TaskCanceledException))
                    LogForModel(item, "Database import or population failed and has been rolled back.", e);

                rollback();
                flushEvents(false);
                throw;
            }

            flushEvents(true);
            return item;
        }, cancellationToken, TaskCreationOptions.HideScheduler, import_scheduler).Unwrap();

        /// <summary>
        /// Exports an item to a legacy (.zip based) package.
        /// </summary>
        /// <param name="item">The item to export.</param>
        public void Export(TModel item)
        {
            var retrievedItem = ModelStore.ConsumableItems.FirstOrDefault(s => s.ID == item.ID);

            if (retrievedItem == null)
                throw new ArgumentException("Specified model could not be found", nameof(item));

            using (var archive = ZipArchive.Create())
            {
                foreach (var file in retrievedItem.Files)
                    archive.AddEntry(file.Filename, Files.Storage.GetStream(file.FileInfo.StoragePath));

                using (var outputStream = exportStorage.GetStream($"{getValidFilename(item.ToString())}{HandledExtensions.First()}", FileAccess.Write, FileMode.Create))
                    archive.SaveTo(outputStream);

                exportStorage.OpenInNativeExplorer();
            }
        }

        /// <summary>
        /// Replace an existing file with a new version.
        /// </summary>
        /// <param name="model">The item to operate on.</param>
        /// <param name="file">The existing file to be replaced.</param>
        /// <param name="contents">The new file contents.</param>
        /// <param name="filename">An optional filename for the new file. Will use the previous filename if not specified.</param>
        public void ReplaceFile(TModel model, TFileModel file, Stream contents, string filename = null)
        {
            using (ContextFactory.GetForWrite())
            {
                DeleteFile(model, file);
                AddFile(model, contents, filename ?? file.Filename);
            }
        }

        /// <summary>
        /// Delete new file.
        /// </summary>
        /// <param name="model">The item to operate on.</param>
        /// <param name="file">The existing file to be deleted.</param>
        public void DeleteFile(TModel model, TFileModel file)
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                // Dereference the existing file info, since the file model will be removed.
                if (file.FileInfo != null)
                {
                    Files.Dereference(file.FileInfo);

                    // This shouldn't be required, but here for safety in case the provided TModel is not being change tracked
                    // Definitely can be removed once we rework the database backend.
                    usage.Context.Set<TFileModel>().Remove(file);
                }

                model.Files.Remove(file);
            }
        }

        /// <summary>
        /// Add a new file.
        /// </summary>
        /// <param name="model">The item to operate on.</param>
        /// <param name="contents">The new file contents.</param>
        /// <param name="filename">The filename for the new file.</param>
        public void AddFile(TModel model, Stream contents, string filename)
        {
            using (ContextFactory.GetForWrite())
            {
                model.Files.Add(new TFileModel
                {
                    Filename = filename,
                    FileInfo = Files.Add(contents)
                });

                Update(model);
            }
        }

        /// <summary>
        /// Perform an update of the specified item.
        /// TODO: Support file additions/removals.
        /// </summary>
        /// <param name="item">The item to update.</param>
        public void Update(TModel item)
        {
            using (ContextFactory.GetForWrite())
            {
                item.Hash = ComputeHash(item);
                ModelStore.Update(item);
            }
        }

        /// <summary>
        /// Delete an item from the manager.
        /// Is a no-op for already deleted items.
        /// </summary>
        /// <param name="item">The item to delete.</param>
        /// <returns>false if no operation was performed</returns>
        public bool Delete(TModel item)
        {
            using (ContextFactory.GetForWrite())
            {
                // re-fetch the model on the import context.
                var foundModel = queryModel().Include(s => s.Files).ThenInclude(f => f.FileInfo).FirstOrDefault(s => s.ID == item.ID);

                if (foundModel == null || foundModel.DeletePending) return false;

                if (ModelStore.Delete(foundModel))
                    Files.Dereference(foundModel.Files.Select(f => f.FileInfo).ToArray());
                return true;
            }
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

            string prefix = reader.Filenames.GetCommonPrefix();
            if (!(prefix.EndsWith('/') || prefix.EndsWith('\\')))
                prefix = string.Empty;

            // import files to manager
            foreach (string file in reader.Filenames)
            {
                using (Stream s = reader.GetStream(file))
                {
                    fileInfos.Add(new TFileModel
                    {
                        Filename = file.Substring(prefix.Length).ToStandardisedPath(),
                        FileInfo = files.Add(s)
                    });
                }
            }

            return fileInfos;
        }

        #region osu-stable import

        /// <summary>
        /// Set a storage with access to an osu-stable install for import purposes.
        /// </summary>
        public Func<Storage> GetStableStorage { private get; set; }

        /// <summary>
        /// Denotes whether an osu-stable installation is present to perform automated imports from.
        /// </summary>
        public bool StableInstallationAvailable => GetStableStorage?.Invoke() != null;

        /// <summary>
        /// The relative path from osu-stable's data directory to import items from.
        /// </summary>
        protected virtual string ImportFromStablePath => null;

        /// <summary>
        /// Select paths to import from stable. Default implementation iterates all directories in <see cref="ImportFromStablePath"/>.
        /// </summary>
        protected virtual IEnumerable<string> GetStableImportPaths(Storage stableStoage) => stableStoage.GetDirectories(ImportFromStablePath);

        /// <summary>
        /// Whether this specified path should be removed after successful import.
        /// </summary>
        /// <param name="path">The path for consideration. May be a file or a directory.</param>
        /// <returns>Whether to perform deletion.</returns>
        protected virtual bool ShouldDeleteArchive(string path) => false;

        /// <summary>
        /// This is a temporary method and will likely be replaced by a full-fledged (and more correctly placed) migration process in the future.
        /// </summary>
        public Task ImportFromStableAsync()
        {
            var stable = GetStableStorage?.Invoke();

            if (stable == null)
            {
                Logger.Log("No osu!stable installation available!", LoggingTarget.Information, LogLevel.Error);
                return Task.CompletedTask;
            }

            if (!stable.ExistsDirectory(ImportFromStablePath))
            {
                // This handles situations like when the user does not have a Skins folder
                Logger.Log($"No {ImportFromStablePath} folder available in osu!stable installation", LoggingTarget.Information, LogLevel.Error);
                return Task.CompletedTask;
            }

            return Task.Run(async () => await Import(GetStableImportPaths(GetStableStorage()).Select(f => stable.GetFullPath(f)).ToArray()));
        }

        #endregion

        /// <summary>
        /// Create a barebones model from the provided archive.
        /// Actual expensive population should be done in <see cref="Populate"/>; this should just prepare for duplicate checking.
        /// </summary>
        /// <param name="archive">The archive to create the model for.</param>
        /// <returns>A model populated with minimal information. Returning a null will abort importing silently.</returns>
        protected abstract TModel CreateModel(ArchiveReader archive);

        /// <summary>
        /// Populate the provided model completely from the given archive.
        /// After this method, the model should be in a state ready to commit to a store.
        /// </summary>
        /// <param name="model">The model to populate.</param>
        /// <param name="archive">The archive to use as a reference for population. May be null.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        protected virtual Task Populate(TModel model, [CanBeNull] ArchiveReader archive, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// Perform any final actions before the import to database executes.
        /// </summary>
        /// <param name="model">The model prepared for import.</param>
        protected virtual void PreImport(TModel model)
        {
        }

        /// <summary>
        /// Check whether an existing model already exists for a new import item.
        /// </summary>
        /// <param name="model">The new model proposed for import.</param>
        /// <returns>An existing model which matches the criteria to skip importing, else null.</returns>
        protected TModel CheckForExisting(TModel model) => model.Hash == null ? null : ModelStore.ConsumableItems.FirstOrDefault(b => b.Hash == model.Hash);

        /// <summary>
        /// After an existing <typeparamref name="TModel"/> is found during an import process, the default behaviour is to use/restore the existing
        /// item and skip the import. This method allows changing that behaviour.
        /// </summary>
        /// <param name="existing">The existing model.</param>
        /// <param name="import">The newly imported model.</param>
        /// <returns>Whether the existing model should be restored and used. Returning false will delete the existing and force a re-import.</returns>
        protected virtual bool CanReuseExisting(TModel existing, TModel import) =>
            // for the best or worst, we copy and import files of a new import before checking whether
            // it is a duplicate. so to check if anything has changed, we can just compare all FileInfo IDs.
            getIDs(existing.Files).SequenceEqual(getIDs(import.Files)) &&
            getFilenames(existing.Files).SequenceEqual(getFilenames(import.Files));

        private IEnumerable<long> getIDs(List<TFileModel> files)
        {
            foreach (var f in files.OrderBy(f => f.Filename))
                yield return f.FileInfo.ID;
        }

        private IEnumerable<string> getFilenames(List<TFileModel> files)
        {
            foreach (var f in files.OrderBy(f => f.Filename))
                yield return f.Filename;
        }

        private DbSet<TModel> queryModel() => ContextFactory.Get().Set<TModel>();

        protected virtual string HumanisedModelName => $"{typeof(TModel).Name.Replace("Info", "").ToLower()}";

        #region Event handling / delaying

        private readonly List<Action> queuedEvents = new List<Action>();

        /// <summary>
        /// Allows delaying of outwards events until an operation is confirmed (at a database level).
        /// </summary>
        private bool delayingEvents;

        /// <summary>
        /// Begin delaying outwards events.
        /// </summary>
        private void delayEvents() => delayingEvents = true;

        /// <summary>
        /// Flush delayed events and disable delaying.
        /// </summary>
        /// <param name="perform">Whether the flushed events should be performed.</param>
        private void flushEvents(bool perform)
        {
            Action[] events;

            lock (queuedEvents)
            {
                events = queuedEvents.ToArray();
                queuedEvents.Clear();
            }

            if (perform)
            {
                foreach (var a in events)
                    a.Invoke();
            }

            delayingEvents = false;
        }

        private void handleEvent(Action a)
        {
            if (delayingEvents)
            {
                lock (queuedEvents)
                    queuedEvents.Add(a);
            }
            else
                a.Invoke();
        }

        #endregion

        private string getValidFilename(string filename)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                filename = filename.Replace(c, '_');
            return filename;
        }
    }
}
