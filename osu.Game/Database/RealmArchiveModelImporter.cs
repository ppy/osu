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
using osu.Game.Extensions;
using osu.Game.IO.Archives;
using osu.Game.Models;
using osu.Game.Overlays.Notifications;
using Realms;

namespace osu.Game.Database
{
    /// <summary>
    /// Encapsulates a model store class to give it import functionality.
    /// Adds cross-functionality with <see cref="RealmFileStore"/> to give access to the central file store for the provided model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class RealmArchiveModelImporter<TModel> : IModelImporter<TModel>
        where TModel : RealmObject, IHasRealmFiles, IHasGuidPrimaryKey, ISoftDelete
    {
        /// <summary>
        /// The maximum number of concurrent imports to run per import scheduler.
        /// </summary>
        private const int import_queue_request_concurrency = 1;

        /// <summary>
        /// The minimum number of items in a single import call in order for the import to be processed as a batch.
        /// Batch imports will apply optimisations preferring speed over consistency when detecting changes in already-imported items.
        /// </summary>
        private const int minimum_items_considered_batch_import = 10;

        /// <summary>
        /// A singleton scheduler shared by all <see cref="RealmArchiveModelImporter{TModel}"/>.
        /// </summary>
        /// <remarks>
        /// This scheduler generally performs IO and CPU intensive work so concurrency is limited harshly.
        /// It is mainly being used as a queue mechanism for large imports.
        /// </remarks>
        private static readonly ThreadedTaskScheduler import_scheduler = new ThreadedTaskScheduler(import_queue_request_concurrency, nameof(RealmArchiveModelImporter<TModel>));

        /// <summary>
        /// A second scheduler for batch imports.
        /// For simplicity, these will just run in parallel with normal priority imports, but a future refactor would see this implemented via a custom scheduler/queue.
        /// See https://gist.github.com/peppy/f0e118a14751fc832ca30dd48ba3876b for an incomplete version of this.
        /// </summary>
        private static readonly ThreadedTaskScheduler import_scheduler_batch = new ThreadedTaskScheduler(import_queue_request_concurrency, nameof(RealmArchiveModelImporter<TModel>));

        /// <summary>
        /// Temporarily pause imports to avoid performance overheads affecting gameplay scenarios.
        /// </summary>
        public bool PauseImports { get; set; }

        public abstract IEnumerable<string> HandledExtensions { get; }

        protected readonly RealmFileStore Files;

        protected readonly RealmAccess Realm;

        /// <summary>
        /// Fired when the user requests to view the resulting import.
        /// </summary>
        public Action<IEnumerable<Live<TModel>>>? PresentImport { get; set; }

        /// <summary>
        /// Set an endpoint for notifications to be posted to.
        /// </summary>
        public Action<Notification>? PostNotification { get; set; }

        protected RealmArchiveModelImporter(Storage storage, RealmAccess realm)
        {
            Realm = realm;

            Files = new RealmFileStore(realm, storage);
        }

        public Task Import(params string[] paths) => Import(paths.Select(p => new ImportTask(p)).ToArray());

        public Task Import(ImportTask[] tasks, ImportParameters parameters = default)
        {
            var notification = new ProgressNotification { State = ProgressNotificationState.Active };

            PostNotification?.Invoke(notification);

            return Import(notification, tasks, parameters);
        }

        public async Task<IEnumerable<Live<TModel>>> Import(ProgressNotification notification, ImportTask[] tasks, ImportParameters parameters = default)
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

            var imported = new List<Live<TModel>>();

            parameters.Batch |= tasks.Length >= minimum_items_considered_batch_import;

            await Task.WhenAll(tasks.Select(async task =>
            {
                if (notification.CancellationToken.IsCancellationRequested)
                    return;

                try
                {
                    var model = await Import(task, parameters, notification.CancellationToken).ConfigureAwait(false);

                    lock (imported)
                    {
                        if (model != null)
                            imported.Add(model);
                        current++;

                        notification.Text = $"Imported {current} of {tasks.Length} {HumanisedModelName}s";
                        notification.Progress = (float)current / tasks.Length;
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    Logger.Error(e, $@"Could not import ({task})", LoggingTarget.Database);
                }
            })).ConfigureAwait(false);

            if (imported.Count == 0)
            {
                if (notification.CancellationToken.IsCancellationRequested)
                {
                    notification.State = ProgressNotificationState.Cancelled;
                    return imported;
                }

                notification.Text = $"{HumanisedModelName.Humanize(LetterCasing.Title)} import failed! Check logs for more information.";
                notification.State = ProgressNotificationState.Cancelled;
            }
            else
            {
                if (tasks.Length > imported.Count)
                    notification.CompletionText = $"Imported {imported.Count} of {tasks.Length} {HumanisedModelName}s.";
                else if (imported.Count > 1)
                    notification.CompletionText = $"Imported {imported.Count} {HumanisedModelName}s!";
                else
                    notification.CompletionText = $"Imported {imported.First().GetDisplayString()}!";

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

        public virtual Task<Live<TModel>?> ImportAsUpdate(ProgressNotification notification, ImportTask task, TModel original) => throw new NotImplementedException();

        /// <summary>
        /// Import one <typeparamref name="TModel"/> from the filesystem and delete the file on success.
        /// Note that this bypasses the UI flow and should only be used for special cases or testing.
        /// </summary>
        /// <param name="task">The <see cref="ImportTask"/> containing data about the <typeparamref name="TModel"/> to import.</param>
        /// <param name="parameters">Parameters to further configure the import process.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The imported model, if successful.</returns>
        public async Task<Live<TModel>?> Import(ImportTask task, ImportParameters parameters = default, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Live<TModel>? import;
            using (ArchiveReader reader = task.GetReader())
                import = await importFromArchive(reader, parameters, cancellationToken).ConfigureAwait(false);

            // We may or may not want to delete the file depending on where it is stored.
            //  e.g. reconstructing/repairing database with items from default storage.
            // Also, not always a single file, i.e. for LegacyFilesystemReader
            // TODO: Add a check to prevent files from storage to be deleted.
            try
            {
                if (import != null && ShouldDeleteArchive(task.Path))
                    task.DeleteFile();
            }
            catch (Exception e)
            {
                Logger.Error(e, $@"Could not delete original file after import ({task})");
            }

            return import;
        }

        /// <summary>
        /// Create and import a model based off the provided <see cref="ArchiveReader"/>.
        /// </summary>
        /// <remarks>
        /// This method also handled queueing the import task on a relevant import thread pool.
        /// </remarks>
        /// <param name="archive">The archive to be imported.</param>
        /// <param name="parameters">Parameters to further configure the import process.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        private async Task<Live<TModel>?> importFromArchive(ArchiveReader archive, ImportParameters parameters = default, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TModel? model = null;

            try
            {
                model = CreateModel(archive, parameters);

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

            var scheduledImport = Task.Factory.StartNew(() => ImportModel(model, archive, parameters, cancellationToken),
                cancellationToken,
                TaskCreationOptions.HideScheduler,
                parameters.Batch ? import_scheduler_batch : import_scheduler);

            return await scheduledImport.ConfigureAwait(false);
        }

        /// <summary>
        /// Silently import an item from a <typeparamref name="TModel"/>.
        /// </summary>
        /// <param name="item">The model to be imported.</param>
        /// <param name="archive">An optional archive to use for model population.</param>
        /// <param name="parameters">Parameters to further configure the import process.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        public virtual Live<TModel>? ImportModel(TModel item, ArchiveReader? archive = null, ImportParameters parameters = default, CancellationToken cancellationToken = default) => Realm.Run(realm =>
        {
            pauseIfNecessary(parameters, cancellationToken);

            TModel? existing;

            if (parameters.Batch && archive != null)
            {
                // this is a fast bail condition to improve large import performance.
                item.Hash = computeHashFast(archive);

                existing = CheckForExisting(item, realm);

                if (existing != null)
                {
                    // bare minimum comparisons
                    //
                    // note that this should really be checking filesizes on disk (of existing files) for some degree of sanity.
                    // or alternatively doing a faster hash check. either of these require database changes and reprocessing of existing files.
                    if (CanSkipImport(existing, item) &&
                        getFilenames(existing.Files).SequenceEqual(getShortenedFilenames(archive).Select(p => p.shortened).Order()) &&
                        checkAllFilesExist(existing))
                    {
                        LogForModel(item, @$"Found existing (optimised) {HumanisedModelName} for {item} (ID {existing.ID}) – skipping import.");

                        using (var transaction = realm.BeginWrite())
                        {
                            UndeleteForReuse(existing);
                            transaction.Commit();
                        }

                        return existing.ToLive(Realm);
                    }

                    LogForModel(item, @"Found existing (optimised) but failed pre-check.");
                }
            }

            try
            {
                // Log output here will be missing a valid hash in non-batch imports.
                LogForModel(item, $@"Beginning import from {archive?.Name ?? "unknown"}...");

                List<RealmNamedFileUsage> files = new List<RealmNamedFileUsage>();

                if (archive != null)
                {
                    // Import files to the disk store.
                    // We intentionally delay adding to realm to avoid blocking on a write during disk operations.
                    foreach (var filenames in getShortenedFilenames(archive))
                    {
                        using (Stream s = archive.GetStream(filenames.original))
                            files.Add(new RealmNamedFileUsage(Files.Add(s, realm, false, parameters.PreferHardLinks), filenames.shortened));
                    }
                }

                using (var transaction = realm.BeginWrite())
                {
                    // Add all files to realm in one go.
                    // This is done ahead of the main transaction to ensure we can correctly cleanup the files, even if the import fails.
                    foreach (var file in files)
                    {
                        if (!file.File.IsManaged)
                            realm.Add(file.File, true);
                    }

                    transaction.Commit();
                }

                item.Files.AddRange(files);
                item.Hash = ComputeHash(item);

                // TODO: do we want to make the transaction this local? not 100% sure, will need further investigation.
                using (var transaction = realm.BeginWrite())
                {
                    // TODO: we may want to run this outside of the transaction.
                    Populate(item, archive, realm, cancellationToken);

                    // Populate() may have adjusted file content (see SkinImporter.updateSkinIniMetadata), so regardless of whether a fast check was done earlier, let's
                    // check for existing items a second time.
                    //
                    // If this is ever a performance issue, the fast-check hash can be compared and trigger a skip of this second check if it still matches.
                    // I don't think it is a huge deal doing a second indexed check, though.
                    existing = CheckForExisting(item, realm);

                    if (existing != null)
                    {
                        if (CanReuseExisting(existing, item))
                        {
                            LogForModel(item, @$"Found existing {HumanisedModelName} for {item} (ID {existing.ID}) – skipping import.");

                            UndeleteForReuse(existing);
                            transaction.Commit();

                            return existing.ToLive(Realm);
                        }

                        LogForModel(item, @"Found existing but failed re-use check.");

                        existing.DeletePending = true;
                    }

                    PreImport(item, realm);

                    // import to store
                    realm.Add(item);

                    PostImport(item, realm, parameters);

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

            return (Live<TModel>?)item.ToLive(Realm);
        });

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
        /// Create a SHA-2 hash from the provided archive based on file content of all files matching <see cref="HashableFileTypes"/>.
        /// </summary>
        /// <remarks>
        ///  In the case of no matching files, a hash will be generated from the passed archive's <see cref="ArchiveReader.Name"/>.
        /// </remarks>
        public string ComputeHash(TModel item)
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

        private string computeHashFast(ArchiveReader reader)
        {
            MemoryStream hashable = new MemoryStream();

            foreach (string? file in reader.Filenames.Where(f => HashableFileTypes.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase))).Order())
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

            return fileInfos;
        }

        private IEnumerable<(string original, string shortened)> getShortenedFilenames(ArchiveReader reader)
        {
            string prefix = reader.Filenames.GetCommonPrefix();
            if (!(prefix.EndsWith('/') || prefix.EndsWith('\\')))
                prefix = string.Empty;

            foreach (string file in reader.Filenames)
                yield return (file, file.Substring(prefix.Length).ToStandardisedPath());
        }

        /// <summary>
        /// Create a barebones model from the provided archive.
        /// Actual expensive population should be done in <see cref="Populate"/>; this should just prepare for duplicate checking.
        /// </summary>
        /// <param name="archive">The archive to create the model for.</param>
        /// <param name="parameters">Parameters to further configure the import process.</param>
        /// <returns>A model populated with minimal information. Returning a null will abort importing silently.</returns>
        protected abstract TModel? CreateModel(ArchiveReader archive, ImportParameters parameters);

        /// <summary>
        /// Populate the provided model completely from the given archive.
        /// After this method, the model should be in a state ready to commit to a store.
        /// </summary>
        /// <param name="model">The model to populate.</param>
        /// <param name="archive">The archive to use as a reference for population. May be null.</param>
        /// <param name="realm">The current realm context.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        protected abstract void Populate(TModel model, ArchiveReader? archive, Realm realm, CancellationToken cancellationToken = default);

        /// <summary>
        /// Perform any final actions before the import to database executes.
        /// </summary>
        /// <param name="model">The model prepared for import.</param>
        /// <param name="realm">The current realm context.</param>
        protected virtual void PreImport(TModel model, Realm realm)
        {
        }

        /// <summary>
        /// Perform any final actions before the import has been committed to the database.
        /// </summary>
        /// <param name="model">The model prepared for import.</param>
        /// <param name="realm">The current realm context.</param>
        /// <param name="parameters">Parameters to further configure the import process.</param>
        protected virtual void PostImport(TModel model, Realm realm, ImportParameters parameters)
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

        private bool checkAllFilesExist(TModel model) =>
            model.Files.All(f => Files.Storage.Exists(f.File.GetStoragePath()));

        /// <summary>
        /// Called when an existing model is in a soft deleted state but being recovered.
        /// </summary>
        /// <param name="existing">The existing model.</param>
        protected virtual void UndeleteForReuse(TModel existing)
        {
            if (!existing.DeletePending)
                return;

            LogForModel(existing, $@"Existing {HumanisedModelName}'s deletion flag has been removed to allow for reuse.");
            existing.DeletePending = false;
        }

        /// <summary>
        /// Whether this specified path should be removed after successful import.
        /// </summary>
        /// <param name="path">The path for consideration. May be a file or a directory.</param>
        /// <returns>Whether to perform deletion.</returns>
        protected virtual bool ShouldDeleteArchive(string path) => false;

        private void pauseIfNecessary(ImportParameters importParameters, CancellationToken cancellationToken)
        {
            if (!PauseImports || importParameters.ImportImmediately)
                return;

            Logger.Log($@"{GetType().Name} is being paused.");

            while (PauseImports)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Thread.Sleep(500);
            }

            cancellationToken.ThrowIfCancellationRequested();
            Logger.Log($@"{GetType().Name} is being resumed.");
        }

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

        public virtual string HumanisedModelName => $"{typeof(TModel).Name.Replace(@"Info", "").ToLowerInvariant()}";
    }
}
