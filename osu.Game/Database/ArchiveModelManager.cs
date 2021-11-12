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
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.IPC;
using osu.Game.Overlays.Notifications;
using SharpCompress.Archives.Zip;

namespace osu.Game.Database
{
    /// <summary>
    /// Encapsulates a model store class to give it import functionality.
    /// Adds cross-functionality with <see cref="FileStore"/> to give access to the central file store for the provided model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TFileModel">The associated file join type.</typeparam>
    public abstract class ArchiveModelManager<TModel, TFileModel> : IModelImporter<TModel>, IModelManager<TModel>, IModelFileManager<TModel, TFileModel>
        where TModel : class, IHasFiles<TFileModel>, IHasPrimaryKey, ISoftDelete
        where TFileModel : class, INamedFileInfo, IHasPrimaryKey, new()
    {
        private const int import_queue_request_concurrency = 1;

        /// <summary>
        /// The size of a batch import operation before considering it a lower priority operation.
        /// </summary>
        private const int low_priority_import_batch_size = 1;

        /// <summary>
        /// A singleton scheduler shared by all <see cref="ArchiveModelManager{TModel,TFileModel}"/>.
        /// </summary>
        /// <remarks>
        /// This scheduler generally performs IO and CPU intensive work so concurrency is limited harshly.
        /// It is mainly being used as a queue mechanism for large imports.
        /// </remarks>
        private static readonly ThreadedTaskScheduler import_scheduler = new ThreadedTaskScheduler(import_queue_request_concurrency, nameof(ArchiveModelManager<TModel, TFileModel>));

        /// <summary>
        /// A second scheduler for lower priority imports.
        /// For simplicity, these will just run in parallel with normal priority imports, but a future refactor would see this implemented via a custom scheduler/queue.
        /// See https://gist.github.com/peppy/f0e118a14751fc832ca30dd48ba3876b for an incomplete version of this.
        /// </summary>
        private static readonly ThreadedTaskScheduler import_scheduler_low_priority = new ThreadedTaskScheduler(import_queue_request_concurrency, nameof(ArchiveModelManager<TModel, TFileModel>));

        public Action<Notification> PostNotification { protected get; set; }

        /// <summary>
        /// Fired when a new or updated <typeparamref name="TModel"/> becomes available in the database.
        /// This is not guaranteed to run on the update thread.
        /// </summary>
        public event Action<TModel> ItemUpdated;

        /// <summary>
        /// Fired when a <typeparamref name="TModel"/> is removed from the database.
        /// This is not guaranteed to run on the update thread.
        /// </summary>
        public event Action<TModel> ItemRemoved;

        public virtual IEnumerable<string> HandledExtensions => new[] { @".zip" };

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
            ModelStore.ItemUpdated += item => handleEvent(() => ItemUpdated?.Invoke(item));
            ModelStore.ItemRemoved += item => handleEvent(() => ItemRemoved?.Invoke(item));

            exportStorage = storage.GetStorageForDirectory(@"exports");

            Files = new FileStore(contextFactory, storage);

            if (importHost != null)
                ipc = new ArchiveImportIPCChannel(importHost, this);

            ModelStore.Cleanup();
        }

        /// <summary>
        /// Import one or more <typeparamref name="TModel"/> items from filesystem <paramref name="paths"/>.
        /// </summary>
        /// <remarks>
        /// This will be treated as a low priority import if more than one path is specified; use <see cref="Import(ImportTask[])"/> to always import at standard priority.
        /// This will post notifications tracking progress.
        /// </remarks>
        /// <param name="paths">One or more archive locations on disk.</param>
        public Task Import(params string[] paths)
        {
            var notification = new ImportProgressNotification();

            PostNotification?.Invoke(notification);

            return Import(notification, paths.Select(p => new ImportTask(p)).ToArray());
        }

        public Task Import(params ImportTask[] tasks)
        {
            var notification = new ImportProgressNotification();

            PostNotification?.Invoke(notification);

            return Import(notification, tasks);
        }

        public async Task<IEnumerable<ILive<TModel>>> Import(ProgressNotification notification, params ImportTask[] tasks)
        {
            if (tasks.Length == 0)
            {
                notification.CompletionText = $"No {HumanisedModelName}s were found to import!";
                notification.State = ProgressNotificationState.Completed;
                return Enumerable.Empty<ILive<TModel>>();
            }

            notification.Progress = 0;
            notification.Text = $"{HumanisedModelName.Humanize(LetterCasing.Title)} import is initialising...";

            int current = 0;

            var imported = new List<ILive<TModel>>();

            bool isLowPriorityImport = tasks.Length > low_priority_import_batch_size;

            try
            {
                await Task.WhenAll(tasks.Select(async task =>
                {
                    notification.CancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var model = await Import(task, isLowPriorityImport, notification.CancellationToken).ConfigureAwait(false);

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
                })).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                if (imported.Count == 0)
                {
                    notification.State = ProgressNotificationState.Cancelled;
                    return imported;
                }
            }

            if (imported.Count == 0)
            {
                notification.Text = $"{HumanisedModelName.Humanize(LetterCasing.Title)} import failed!";
                notification.State = ProgressNotificationState.Cancelled;
            }
            else
            {
                notification.CompletionText = imported.Count == 1
                    ? $"Imported {imported.First().Value.GetDisplayString()}!"
                    : $"Imported {imported.Count} {HumanisedModelName}s!";

                if (imported.Count > 0 && PostImport != null)
                {
                    notification.CompletionText += " Click to view.";
                    notification.CompletionClickAction = () =>
                    {
                        PostImport?.Invoke(imported);
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
        /// <param name="lowPriority">Whether this is a low priority import.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The imported model, if successful.</returns>
        public async Task<ILive<TModel>> Import(ImportTask task, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ILive<TModel> import;
            using (ArchiveReader reader = task.GetReader())
                import = await Import(reader, lowPriority, cancellationToken).ConfigureAwait(false);

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
                LogForModel(import?.Value, $@"Could not delete original file after import ({task})", e);
            }

            return import;
        }

        public Action<IEnumerable<ILive<TModel>>> PostImport { protected get; set; }

        /// <summary>
        /// Silently import an item from an <see cref="ArchiveReader"/>.
        /// </summary>
        /// <param name="archive">The archive to be imported.</param>
        /// <param name="lowPriority">Whether this is a low priority import.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        public Task<ILive<TModel>> Import(ArchiveReader archive, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TModel model = null;

            try
            {
                model = CreateModel(archive);

                if (model == null)
                    return Task.FromResult<ILive<TModel>>(null);
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                LogForModel(model, @$"Model creation of {archive.Name} failed.", e);
                return null;
            }

            return Import(model, archive, lowPriority, cancellationToken);
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
        /// Whether the implementation overrides <see cref="ComputeHash"/> with a custom implementation.
        /// Custom hash implementations must bypass the early exit in the import flow (see <see cref="computeHashFast"/> usage).
        /// </summary>
        protected virtual bool HasCustomHashFunction => false;

        /// <summary>
        /// Create a SHA-2 hash from the provided archive based on file content of all files matching <see cref="HashableFileTypes"/>.
        /// </summary>
        /// <remarks>
        ///  In the case of no matching files, a hash will be generated from the passed archive's <see cref="ArchiveReader.Name"/>.
        /// </remarks>
        protected virtual string ComputeHash(TModel item)
        {
            var hashableFiles = item.Files
                                    .Where(f => HashableFileTypes.Any(ext => f.Filename.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                                    .OrderBy(f => f.Filename)
                                    .ToArray();

            if (hashableFiles.Length > 0)
            {
                // for now, concatenate all hashable files in the set to create a unique hash.
                MemoryStream hashable = new MemoryStream();

                foreach (TFileModel file in hashableFiles)
                {
                    using (Stream s = Files.Store.GetStream(file.FileInfo.StoragePath))
                        s.CopyTo(hashable);
                }

                if (hashable.Length > 0)
                    return hashable.ComputeSHA2Hash();
            }

            return generateFallbackHash();
        }

        /// <summary>
        /// Silently import an item from a <typeparamref name="TModel"/>.
        /// </summary>
        /// <param name="item">The model to be imported.</param>
        /// <param name="archive">An optional archive to use for model population.</param>
        /// <param name="lowPriority">Whether this is a low priority import.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        public virtual async Task<ILive<TModel>> Import(TModel item, ArchiveReader archive = null, bool lowPriority = false, CancellationToken cancellationToken = default) => await Task.Factory.StartNew(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            bool checkedExisting = false;
            TModel existing = null;

            if (archive != null && !HasCustomHashFunction)
            {
                // this is a fast bail condition to improve large import performance.
                item.Hash = computeHashFast(archive);

                checkedExisting = true;
                existing = CheckForExisting(item);

                if (existing != null)
                {
                    // bare minimum comparisons
                    //
                    // note that this should really be checking filesizes on disk (of existing files) for some degree of sanity.
                    // or alternatively doing a faster hash check. either of these require database changes and reprocessing of existing files.
                    if (CanSkipImport(existing, item) &&
                        getFilenames(existing.Files).SequenceEqual(getShortenedFilenames(archive).Select(p => p.shortened).OrderBy(f => f)))
                    {
                        LogForModel(item, @$"Found existing (optimised) {HumanisedModelName} for {item} (ID {existing.ID}) – skipping import.");
                        Undelete(existing);
                        return existing.ToEntityFrameworkLive();
                    }

                    LogForModel(item, @"Found existing (optimised) but failed pre-check.");
                }
            }

            void rollback()
            {
                if (!Delete(item))
                {
                    // We may have not yet added the model to the underlying table, but should still clean up files.
                    LogForModel(item, @"Dereferencing files for incomplete import.");
                    Files.Dereference(item.Files.Select(f => f.FileInfo).ToArray());
                }
            }

            delayEvents();

            try
            {
                LogForModel(item, @"Beginning import...");

                item.Files = archive != null ? createFileInfos(archive, Files) : new List<TFileModel>();
                item.Hash = ComputeHash(item);

                await Populate(item, archive, cancellationToken).ConfigureAwait(false);

                using (var write = ContextFactory.GetForWrite()) // used to share a context for full import. keep in mind this will block all writes.
                {
                    try
                    {
                        if (!write.IsTransactionLeader) throw new InvalidOperationException(@$"Ensure there is no parent transaction so errors can correctly be handled by {this}");

                        if (!checkedExisting)
                            existing = CheckForExisting(item);

                        if (existing != null)
                        {
                            if (CanReuseExisting(existing, item))
                            {
                                Undelete(existing);
                                LogForModel(item, @$"Found existing {HumanisedModelName} for {item} (ID {existing.ID}) – skipping import.");
                                // existing item will be used; rollback new import and exit early.
                                rollback();
                                flushEvents(true);
                                return existing.ToEntityFrameworkLive();
                            }

                            LogForModel(item, @"Found existing but failed re-use check.");
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

                LogForModel(item, @"Import successfully completed!");
            }
            catch (Exception e)
            {
                if (!(e is TaskCanceledException))
                    LogForModel(item, @"Database import or population failed and has been rolled back.", e);

                rollback();
                flushEvents(false);
                throw;
            }

            flushEvents(true);
            return item.ToEntityFrameworkLive();
        }, cancellationToken, TaskCreationOptions.HideScheduler, lowPriority ? import_scheduler_low_priority : import_scheduler).Unwrap().ConfigureAwait(false);

        /// <summary>
        /// Exports an item to a legacy (.zip based) package.
        /// </summary>
        /// <param name="item">The item to export.</param>
        public void Export(TModel item)
        {
            var retrievedItem = ModelStore.ConsumableItems.FirstOrDefault(s => s.ID == item.ID);

            if (retrievedItem == null)
                throw new ArgumentException(@"Specified model could not be found", nameof(item));

            string filename = $"{GetValidFilename(item.ToString())}{HandledExtensions.First()}";

            using (var stream = exportStorage.GetStream(filename, FileAccess.Write, FileMode.Create))
                ExportModelTo(retrievedItem, stream);

            exportStorage.PresentFileExternally(filename);
        }

        /// <summary>
        /// Exports an item to the given output stream.
        /// </summary>
        /// <param name="model">The item to export.</param>
        /// <param name="outputStream">The output stream to export to.</param>
        public virtual void ExportModelTo(TModel model, Stream outputStream)
        {
            using (var archive = ZipArchive.Create())
            {
                foreach (var file in model.Files)
                    archive.AddEntry(file.Filename, Files.Storage.GetStream(file.FileInfo.StoragePath));

                archive.SaveTo(outputStream);
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
        /// Delete an existing file.
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

                    if (file.ID > 0)
                    {
                        // This shouldn't be required, but here for safety in case the provided TModel is not being change tracked
                        // Definitely can be removed once we rework the database backend.
                        usage.Context.Set<TFileModel>().Remove(file);
                    }
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
            }

            if (model.ID > 0)
                Update(model);
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

        private string computeHashFast(ArchiveReader reader)
        {
            MemoryStream hashable = new MemoryStream();

            foreach (string file in reader.Filenames.Where(f => HashableFileTypes.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase))).OrderBy(f => f))
            {
                using (Stream s = reader.GetStream(file))
                    s.CopyTo(hashable);
            }

            if (hashable.Length > 0)
                return hashable.ComputeSHA2Hash();

            return generateFallbackHash();
        }

        /// <summary>
        /// Create all required <see cref="IO.FileInfo"/>s for the provided archive, adding them to the global file store.
        /// </summary>
        private List<TFileModel> createFileInfos(ArchiveReader reader, FileStore files)
        {
            var fileInfos = new List<TFileModel>();

            // import files to manager
            foreach (var filenames in getShortenedFilenames(reader))
            {
                using (Stream s = reader.GetStream(filenames.original))
                {
                    fileInfos.Add(new TFileModel
                    {
                        Filename = filenames.shortened,
                        FileInfo = files.Add(s)
                    });
                }
            }

            return fileInfos;
        }

        private IEnumerable<(string original, string shortened)> getShortenedFilenames(ArchiveReader reader)
        {
            string prefix = reader.Filenames.GetCommonPrefix();
            if (!(prefix.EndsWith('/') || prefix.EndsWith('\\')))
                prefix = string.Empty;

            // import files to manager
            foreach (string file in reader.Filenames)
                yield return (file, file.Substring(prefix.Length).ToStandardisedPath());
        }

        #region osu-stable import

        /// <summary>
        /// The relative path from osu-stable's data directory to import items from.
        /// </summary>
        protected virtual string ImportFromStablePath => null;

        /// <summary>
        /// Select paths to import from stable where all paths should be absolute. Default implementation iterates all directories in <see cref="ImportFromStablePath"/>.
        /// </summary>
        protected virtual IEnumerable<string> GetStableImportPaths(Storage storage) => storage.GetDirectories(ImportFromStablePath)
                                                                                              .Select(path => storage.GetFullPath(path));

        /// <summary>
        /// Whether this specified path should be removed after successful import.
        /// </summary>
        /// <param name="path">The path for consideration. May be a file or a directory.</param>
        /// <returns>Whether to perform deletion.</returns>
        protected virtual bool ShouldDeleteArchive(string path) => false;

        public Task ImportFromStableAsync(StableStorage stableStorage)
        {
            var storage = PrepareStableStorage(stableStorage);

            // Handle situations like when the user does not have a Skins folder.
            if (!storage.ExistsDirectory(ImportFromStablePath))
            {
                string fullPath = storage.GetFullPath(ImportFromStablePath);

                Logger.Log(@$"Folder ""{fullPath}"" not available in the target osu!stable installation to import {HumanisedModelName}s.", LoggingTarget.Information, LogLevel.Error);
                return Task.CompletedTask;
            }

            return Task.Run(async () => await Import(GetStableImportPaths(storage).ToArray()).ConfigureAwait(false));
        }

        /// <summary>
        /// Run any required traversal operations on the stable storage location before performing operations.
        /// </summary>
        /// <param name="stableStorage">The stable storage.</param>
        /// <returns>The usable storage. Return the unchanged <paramref name="stableStorage"/> if no traversal is required.</returns>
        protected virtual Storage PrepareStableStorage(StableStorage stableStorage) => stableStorage;

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
        protected abstract Task Populate(TModel model, [CanBeNull] ArchiveReader archive, CancellationToken cancellationToken = default);

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

        public bool IsAvailableLocally(TModel model) => CheckLocalAvailability(model, ModelStore.ConsumableItems.Where(m => !m.DeletePending));

        /// <summary>
        /// Performs implementation specific comparisons to determine whether a given model is present in the local store.
        /// </summary>
        /// <param name="model">The <typeparamref name="TModel"/> whose existence needs to be checked.</param>
        /// <param name="items">The usable items present in the store.</param>
        /// <returns>Whether the <typeparamref name="TModel"/> exists.</returns>
        protected virtual bool CheckLocalAvailability(TModel model, IQueryable<TModel> items)
            => model.ID > 0 && items.Any(i => i.ID == model.ID && i.Files.Any());

        /// <summary>
        /// Whether import can be skipped after finding an existing import early in the process.
        /// Only valid when <see cref="ComputeHash"/> is not overridden.
        /// </summary>
        /// <param name="existing">The existing model.</param>
        /// <param name="import">The newly imported model.</param>
        /// <returns>Whether to skip this import completely.</returns>
        protected virtual bool CanSkipImport(TModel existing, TModel import) => true;

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

        public virtual string HumanisedModelName => $"{typeof(TModel).Name.Replace(@"Info", "").ToLower()}";

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

        private static string generateFallbackHash()
        {
            // if a hash could no be generated from file content, presume a unique / new import.
            // therefore, let's use a guaranteed unique hash.
            // this doesn't follow the SHA2 hashing schema intentionally, so such entries on the data store can be identified.
            return Guid.NewGuid().ToString();
        }

        private readonly char[] invalidFilenameCharacters = Path.GetInvalidFileNameChars()
                                                                // Backslash is added to avoid issues when exporting to zip.
                                                                // See SharpCompress filename normalisation https://github.com/adamhathcock/sharpcompress/blob/a1e7c0068db814c9aa78d86a94ccd1c761af74bd/src/SharpCompress/Writers/Zip/ZipWriter.cs#L143.
                                                                .Append('\\')
                                                                .ToArray();

        protected string GetValidFilename(string filename)
        {
            foreach (char c in invalidFilenameCharacters)
                filename = filename.Replace(c, '_');
            return filename;
        }
    }
}
