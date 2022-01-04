// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.IO.Archives;
using osu.Game.Models;
using osu.Game.Overlays.Notifications;
using Realms;

#nullable enable

namespace osu.Game.Stores
{
    /// <summary>
    /// Encapsulates a model store class to give it import functionality.
    /// Adds cross-functionality with <see cref="RealmFileStore"/> to give access to the central file store for the provided model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class RealmArchiveModelImporter<TModel> : IModelImporter<TModel>
        where TModel : RealmObject, IHasRealmFiles, IHasGuidPrimaryKey, ISoftDelete
    {
        private const int import_queue_request_concurrency = 1;

        /// <summary>
        /// The size of a batch import operation before considering it a lower priority operation.
        /// </summary>
        private const int low_priority_import_batch_size = 1;

        /// <summary>
        /// A singleton scheduler shared by all <see cref="RealmArchiveModelImporter{TModel}"/>.
        /// </summary>
        /// <remarks>
        /// This scheduler generally performs IO and CPU intensive work so concurrency is limited harshly.
        /// It is mainly being used as a queue mechanism for large imports.
        /// </remarks>
        private static readonly ThreadedTaskScheduler import_scheduler = new ThreadedTaskScheduler(import_queue_request_concurrency, nameof(RealmArchiveModelImporter<TModel>));

        /// <summary>
        /// A second scheduler for lower priority imports.
        /// For simplicity, these will just run in parallel with normal priority imports, but a future refactor would see this implemented via a custom scheduler/queue.
        /// See https://gist.github.com/peppy/f0e118a14751fc832ca30dd48ba3876b for an incomplete version of this.
        /// </summary>
        private static readonly ThreadedTaskScheduler import_scheduler_low_priority = new ThreadedTaskScheduler(import_queue_request_concurrency, nameof(RealmArchiveModelImporter<TModel>));

        public virtual IEnumerable<string> HandledExtensions => new[] { @".zip" };

        protected readonly RealmFileStore Files;

        protected readonly RealmContextFactory ContextFactory;

        /// <summary>
        /// Fired when the user requests to view the resulting import.
        /// </summary>
        public Action<IEnumerable<ILive<TModel>>>? PostImport { get; set; }

        /// <summary>
        /// Set an endpoint for notifications to be posted to.
        /// </summary>
        public Action<Notification>? PostNotification { protected get; set; }

        protected RealmArchiveModelImporter(Storage storage, RealmContextFactory contextFactory)
        {
            ContextFactory = contextFactory;

            Files = new RealmFileStore(contextFactory, storage);
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
            var notification = new ProgressNotification { State = ProgressNotificationState.Active };

            PostNotification?.Invoke(notification);

            return Import(notification, paths.Select(p => new ImportTask(p)).ToArray());
        }

        public Task Import(params ImportTask[] tasks)
        {
            var notification = new ProgressNotification { State = ProgressNotificationState.Active };

            PostNotification?.Invoke(notification);

            return Import(notification, tasks);
        }

        public async Task<IEnumerable<ILive<TModel>>> Import(ProgressNotification notification, params ImportTask[] tasks)
        {
            if (tasks.Length == 0)
            {
                notification.CompletionText = $"No {HumanisedModelName}s were found to import!";
                notification.State = ProgressNotificationState.Completed;
                return Enumerable.Empty<RealmLive<TModel>>();
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
                    ? $"Imported {imported.First()}!"
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
        public async Task<ILive<TModel>?> Import(ImportTask task, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ILive<TModel>? import;
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
                Logger.Error(e, $@"Could not delete original file after import ({task})");
            }

            return import;
        }

        /// <summary>
        /// Silently import an item from an <see cref="ArchiveReader"/>.
        /// </summary>
        /// <param name="archive">The archive to be imported.</param>
        /// <param name="lowPriority">Whether this is a low priority import.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        public async Task<ILive<TModel>?> Import(ArchiveReader archive, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TModel? model = null;

            try
            {
                model = CreateModel(archive);

                if (model == null)
                    return null;
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

            var scheduledImport = Task.Factory.StartNew(async () => await Import(model, archive, lowPriority, cancellationToken).ConfigureAwait(false),
                cancellationToken, TaskCreationOptions.HideScheduler, lowPriority ? import_scheduler_low_priority : import_scheduler).Unwrap();

            return await scheduledImport.ConfigureAwait(false);
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

        internal static void LogForModel(TModel? model, string message, Exception? e = null)
        {
            string trimmedHash;
            if (model == null || !model.IsValid || string.IsNullOrEmpty(model.Hash))
                trimmedHash = "?????";
            else
                trimmedHash = model.Hash.Substring(0, 5);

            string prefix = $"[{trimmedHash}]";

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
            // for now, concatenate all hashable files in the set to create a unique hash.
            MemoryStream hashable = new MemoryStream();

            foreach (RealmNamedFileUsage file in item.Files.Where(f => HashableFileTypes.Any(ext => f.Filename.EndsWith(ext, StringComparison.OrdinalIgnoreCase))).OrderBy(f => f.Filename))
            {
                using (Stream s = Files.Store.GetStream(file.File.GetStoragePath()))
                    s.CopyTo(hashable);
            }

            if (hashable.Length > 0)
                return hashable.ComputeSHA2Hash();

            return item.Hash;
        }

        /// <summary>
        /// Silently import an item from a <typeparamref name="TModel"/>.
        /// </summary>
        /// <param name="item">The model to be imported.</param>
        /// <param name="archive">An optional archive to use for model population.</param>
        /// <param name="lowPriority">Whether this is a low priority import.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        public virtual async Task<ILive<TModel>?> Import(TModel item, ArchiveReader? archive = null, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            using (var realm = ContextFactory.CreateContext())
            {
                cancellationToken.ThrowIfCancellationRequested();

                bool checkedExisting = false;
                TModel? existing = null;

                if (archive != null && !HasCustomHashFunction)
                {
                    // this is a fast bail condition to improve large import performance.
                    item.Hash = computeHashFast(archive);

                    checkedExisting = true;
                    existing = CheckForExisting(item, realm);

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

                            using (var transaction = realm.BeginWrite())
                            {
                                existing.DeletePending = false;
                                transaction.Commit();
                            }

                            return existing.ToLive(ContextFactory);
                        }

                        LogForModel(item, @"Found existing (optimised) but failed pre-check.");
                    }
                }

                try
                {
                    LogForModel(item, @"Beginning import...");

                    // TODO: do we want to make the transaction this local? not 100% sure, will need further investigation.
                    using (var transaction = realm.BeginWrite())
                    {
                        if (archive != null)
                            // TODO: look into rollback of file additions (or delayed commit).
                            item.Files.AddRange(createFileInfos(archive, Files, realm));

                        item.Hash = ComputeHash(item);

                        // TODO: we may want to run this outside of the transaction.
                        await Populate(item, archive, realm, cancellationToken).ConfigureAwait(false);

                        if (!checkedExisting)
                            existing = CheckForExisting(item, realm);

                        if (existing != null)
                        {
                            if (CanReuseExisting(existing, item))
                            {
                                LogForModel(item, @$"Found existing {HumanisedModelName} for {item} (ID {existing.ID}) – skipping import.");

                                existing.DeletePending = false;
                                transaction.Commit();

                                return existing.ToLive(ContextFactory);
                            }

                            LogForModel(item, @"Found existing but failed re-use check.");

                            existing.DeletePending = true;

                            // todo: actually delete? i don't think this is required...
                            // ModelStore.PurgeDeletable(s => s.ID == existing.ID);
                        }

                        PreImport(item, realm);

                        // import to store
                        realm.Add(item);

                        transaction.Commit();
                    }

                    LogForModel(item, @"Import successfully completed!");
                }
                catch (Exception e)
                {
                    if (!(e is TaskCanceledException))
                        LogForModel(item, @"Database import or population failed and has been rolled back.", e);

                    throw;
                }

                return item.ToLive(ContextFactory);
            }
        }

        private string computeHashFast(ArchiveReader reader)
        {
            MemoryStream hashable = new MemoryStream();

            foreach (string? file in reader.Filenames.Where(f => HashableFileTypes.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase))).OrderBy(f => f))
            {
                using (Stream s = reader.GetStream(file))
                    s.CopyTo(hashable);
            }

            if (hashable.Length > 0)
                return hashable.ComputeSHA2Hash();

            return reader.Name.ComputeSHA2Hash();
        }

        /// <summary>
        /// Create all required <see cref="File"/>s for the provided archive, adding them to the global file store.
        /// </summary>
        private List<RealmNamedFileUsage> createFileInfos(ArchiveReader reader, RealmFileStore files, Realm realm)
        {
            var fileInfos = new List<RealmNamedFileUsage>();

            // import files to manager
            foreach (var filenames in getShortenedFilenames(reader))
            {
                using (Stream s = reader.GetStream(filenames.original))
                {
                    var item = new RealmNamedFileUsage(files.Add(s, realm), filenames.shortened);
                    fileInfos.Add(item);
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

        /// <summary>
        /// Create a barebones model from the provided archive.
        /// Actual expensive population should be done in <see cref="Populate"/>; this should just prepare for duplicate checking.
        /// </summary>
        /// <param name="archive">The archive to create the model for.</param>
        /// <returns>A model populated with minimal information. Returning a null will abort importing silently.</returns>
        protected abstract TModel? CreateModel(ArchiveReader archive);

        /// <summary>
        /// Populate the provided model completely from the given archive.
        /// After this method, the model should be in a state ready to commit to a store.
        /// </summary>
        /// <param name="model">The model to populate.</param>
        /// <param name="archive">The archive to use as a reference for population. May be null.</param>
        /// <param name="realm">The current realm context.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        protected abstract Task Populate(TModel model, ArchiveReader? archive, Realm realm, CancellationToken cancellationToken = default);

        /// <summary>
        /// Perform any final actions before the import to database executes.
        /// </summary>
        /// <param name="model">The model prepared for import.</param>
        /// <param name="realm">The current realm context.</param>
        protected virtual void PreImport(TModel model, Realm realm)
        {
        }

        /// <summary>
        /// Check whether an existing model already exists for a new import item.
        /// </summary>
        /// <param name="model">The new model proposed for import.</param>
        /// <param name="realm">The current realm context.</param>
        /// <returns>An existing model which matches the criteria to skip importing, else null.</returns>
        protected TModel? CheckForExisting(TModel model, Realm realm) => string.IsNullOrEmpty(model.Hash) ? null : realm.All<TModel>().FirstOrDefault(b => b.Hash == model.Hash);

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
            // it is a duplicate. so to check if anything has changed, we can just compare all File IDs.
            getIDs(existing.Files).SequenceEqual(getIDs(import.Files)) &&
            getFilenames(existing.Files).SequenceEqual(getFilenames(import.Files));

        /// <summary>
        /// Whether this specified path should be removed after successful import.
        /// </summary>
        /// <param name="path">The path for consideration. May be a file or a directory.</param>
        /// <returns>Whether to perform deletion.</returns>
        protected virtual bool ShouldDeleteArchive(string path) => false;

        private IEnumerable<string> getIDs(IEnumerable<INamedFile> files)
        {
            foreach (var f in files.OrderBy(f => f.Filename))
                yield return f.File.Hash;
        }

        private IEnumerable<string> getFilenames(IEnumerable<INamedFile> files)
        {
            foreach (var f in files.OrderBy(f => f.Filename))
                yield return f.Filename;
        }

        public virtual string HumanisedModelName => $"{typeof(TModel).Name.Replace(@"Info", "").ToLower()}";
    }
}
