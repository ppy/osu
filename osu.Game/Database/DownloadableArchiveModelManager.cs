// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Online.API;
using osu.Game.Overlays.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace osu.Game.Database
{
    /// <summary>
    /// An <see cref="ArchiveModelManager{TModel, TFileModel}"/> that has the ability to download models using an <see cref="IAPIProvider"/> and
    /// import them into the store.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TFileModel">The associated file join type.</typeparam>
    public abstract class DownloadableArchiveModelManager<TModel, TFileModel> : ArchiveModelManager<TModel, TFileModel>, IModelDownloader<TModel>
        where TModel : class, IHasFiles<TFileModel>, IHasPrimaryKey, ISoftDelete, IEquatable<TModel>
        where TFileModel : INamedFileInfo, new()
    {
        public event Action<ArchiveDownloadRequest<TModel>> DownloadBegan;

        public event Action<ArchiveDownloadRequest<TModel>> DownloadFailed;

        private readonly IAPIProvider api;

        private readonly List<DownloadRequestPackage<TModel>> currentDownloads = new List<DownloadRequestPackage<TModel>>();

        private readonly MutableDatabaseBackedStoreWithFileIncludes<TModel, TFileModel> modelStore;

        protected DownloadableArchiveModelManager(Storage storage, IDatabaseContextFactory contextFactory, IAPIProvider api, MutableDatabaseBackedStoreWithFileIncludes<TModel, TFileModel> modelStore,
                                                  IIpcHost importHost = null)
            : base(storage, contextFactory, modelStore, importHost)
        {
            this.api = api;
            this.modelStore = modelStore;
        }

        /// <summary>
        /// Creates the download request for this <typeparamref name="TModel"/>.
        /// </summary>
        /// <param name="model">The <typeparamref name="TModel"/> to be downloaded.</param>
        /// <param name="minimiseDownloadSize">Whether this download should be optimised for slow connections. Generally means extras are not included in the download bundle.</param>
        /// <returns>The request object.</returns>
        protected abstract ArchiveDownloadRequest<TModel> CreateDownloadRequest(TModel model, bool minimiseDownloadSize);

        /// <summary>
        /// Begin a download for the requested <typeparamref name="TModel"/>.
        /// </summary>
        /// <param name="model">The <typeparamref name="TModel"/> to be downloaded.</param>
        /// <param name="minimiseDownloadSize">Whether this download should be optimised for slow connections. Generally means extras are not included in the download bundle.</param>
        /// <returns>Whether the download was started.</returns>
        public bool Download(TModel model, bool minimiseDownloadSize = false)
        {
            if (!canDownload(model)) return false;

            var package = new DownloadRequestPackage<TModel>
            {
                Request = CreateDownloadRequest(model, minimiseDownloadSize),
                Notification = new DownloadNotification { Text = $"Downloading {model}" },
            };

            var request = package.Request;
            var notification = package.Notification;

            request.DownloadProgressed += progress =>
            {
                notification.State = ProgressNotificationState.Active;
                notification.Progress = progress;
            };

            request.Success += filename =>
            {
                Task.Factory.StartNew(async () =>
                {
                    // This gets scheduled back to the update thread, but we want the import to run in the background.
                    var imported = await Import(notification, filename);

                    // for now a failed import will be marked as a failed download for simplicity.
                    if (!imported.Any())
                        DownloadFailed?.Invoke(request);

                    currentDownloads.Remove(package);
                }, TaskCreationOptions.LongRunning);
            };

            request.Failure += triggerFailure;

            notification.CancelRequested += () =>
            {
                request.Cancel();
                currentDownloads.Remove(request);
                notification.State = ProgressNotificationState.Cancelled;
                return true;
            };

            currentDownloads.Add(package);
            PostNotification?.Invoke(notification);

            api.PerformAsync(request);

            DownloadBegan?.Invoke(request);
            return true;

            void triggerFailure(Exception error)
            {
                DownloadFailed?.Invoke(request);

                if (error is OperationCanceledException) return;

                notification.State = ProgressNotificationState.Cancelled;
                Logger.Error(error, $"{HumanisedModelName.Titleize()} download failed!");
                currentDownloads.Remove(package);
            }
        }

        public bool IsAvailableLocally(TModel model) => CheckLocalAvailability(model, modelStore.ConsumableItems.Where(m => !m.DeletePending));

        /// <summary>
        /// Performs implementation specific comparisons to determine whether a given model is present in the local store.
        /// </summary>
        /// <param name="model">The <typeparamref name="TModel"/> whose existence needs to be checked.</param>
        /// <param name="items">The usable items present in the store.</param>
        /// <returns>Whether the <typeparamref name="TModel"/> exists.</returns>
        protected virtual bool CheckLocalAvailability(TModel model, IQueryable<TModel> items)
            => model.ID > 0 && items.Any(i => i.ID == model.ID && i.Files.Any());

        public ArchiveDownloadRequest<TModel> GetExistingDownload(TModel model) => currentDownloads.Find(p => p.Request.Model.Equals(model)).Request;

        private bool canDownload(TModel model) => GetExistingDownload(model) == null && api != null;
    }
}
