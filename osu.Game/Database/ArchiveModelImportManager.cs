using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.IO;
using osu.Game.IO;
using osu.Game.IPC;
using osu.Game.Overlays.Notifications;
using FileInfo = osu.Game.IO.FileInfo;

namespace osu.Game.Database
{
    public abstract class ArchiveModelImportManager<TModel, TFileModel> : ICanImportArchives
        where TModel : class, IHasFiles<TFileModel>, IHasPrimaryKey, ISoftDelete
        where TFileModel : INamedFileInfo, new()
    {
        /// <summary>
        /// Set an endpoint for notifications to be posted to.
        /// </summary>
        public Action<Notification> PostNotification { protected get; set; }

        public virtual string[] HandledExtensions => new[] { ".zip" };

        protected readonly FileStore Files;

        protected readonly IDatabaseContextFactory ContextFactory;

        protected readonly IMutableStore<TModel> ModelStore;

        // ReSharper disable once NotAccessedField.Local (we should keep a reference to this so it is not finalised)
        private ArchiveImportIPCChannel ipc;

        protected ArchiveModelImportManager(Storage storage, IDatabaseContextFactory contextFactory, IMutableStore<TModel> modelStore, IIpcHost importHost = null)
        {
            ContextFactory = contextFactory;
            ModelStore = modelStore;
            Files = new FileStore(contextFactory, storage);

            if (importHost != null)
                ipc = new ArchiveImportIPCChannel(importHost, this);
        }

        /// <summary>
        /// Import one or more <see cref="BeatmapSetInfo"/> from filesystem <paramref name="paths"/>.
        /// This will post notifications tracking progress.
        /// </summary>
        /// <param name="paths">One or more beatmap locations on disk.</param>
        public void Import(params string[] paths)
        {
            var notification = new ProgressNotification
            {
                Text = "Import is initialising...",
                CompletionText = "Import successful!",
                Progress = 0,
                State = ProgressNotificationState.Active,
            };

            PostNotification?.Invoke(notification);

            List<TModel> imported = new List<TModel>();

            int i = 0;
            foreach (string path in paths)
            {
                if (notification.State == ProgressNotificationState.Cancelled)
                    // user requested abort
                    return;

                try
                {
                    notification.Text = $"Importing ({i} of {paths.Length})\n{Path.GetFileName(path)}";
                    using (ArchiveReader reader = getReaderFrom(path))
                        imported.Add(Import(reader));

                    notification.Progress = (float)++i / paths.Length;

                    // We may or may not want to delete the file depending on where it is stored.
                    //  e.g. reconstructing/repairing database with beatmaps from default storage.
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
                    Logger.Error(e, $@"Could not import beatmap set ({Path.GetFileName(path)})");
                }
            }

            notification.State = ProgressNotificationState.Completed;
        }

        /// <summary>
        /// Import a model from an <see cref="ArchiveReader"/>.
        /// </summary>
        /// <param name="archive">The beatmap to be imported.</param>
        public TModel Import(ArchiveReader archive)
        {
            using (ContextFactory.GetForWrite()) // used to share a context for full import. keep in mind this will block all writes.
            {
                // create a new set info (don't yet add to database)
                var model = CreateModel(archive);

                var existing = CheckForExisting(model);

                if (existing != null) return existing;

                model.Files = createFileInfos(archive, Files);

                Populate(model, archive);

                // import to store
                ModelStore.Add(model);

                return model;
            }
        }

        /// <summary>
        /// Delete a model from the manager.
        /// Is a no-op for already deleted models.
        /// </summary>
        /// <param name="model">The model to delete.</param>
        public void Delete(TModel model)
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                var context = usage.Context;

                context.ChangeTracker.AutoDetectChangesEnabled = false;

                // re-fetch the model on the import context.
                var foundModel = ContextFactory.Get().Set<TModel>().Include(s => s.Files).ThenInclude(f => f.FileInfo).First(s => s.ID == model.ID);

                if (foundModel.DeletePending || !CheckCanDelete(foundModel)) return;

                if (ModelStore.Delete(foundModel))
                    Files.Dereference(foundModel.Files.Select(f => f.FileInfo).ToArray());

                context.ChangeTracker.AutoDetectChangesEnabled = true;
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
        /// <param name="archive"></param>
        /// <returns></returns>
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

        protected virtual bool CheckCanDelete(TModel model) => true;

        /// <summary>
        /// Creates an <see cref="ArchiveReader"/> from a valid storage path.
        /// </summary>
        /// <param name="path">A file or folder path resolving the archive content.</param>
        /// <returns>A reader giving access to the archive's content.</returns>
        private ArchiveReader getReaderFrom(string path)
        {
            if (ZipFile.IsZipFile(path))
                return new OszArchiveReader(Files.Storage.GetStream(path), Path.GetFileName(path));
            return new LegacyFilesystemReader(path);
        }
    }
}
