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
        where TModel : class, IHasFiles<TFileModel>, IHasPrimaryKey, ISoftDelete
        where TFileModel : INamedFileInfo, new()
    {
        public event Action<ArchiveDownloadRequest<TModel>> DownloadBegan;

        public event Action<ArchiveDownloadRequest<TModel>> DownloadFailed;

        private readonly IAPIProvider api;

        private readonly List<ArchiveDownloadRequest<TModel>> currentDownloads = new List<ArchiveDownloadRequest<TModel>>();

        private readonly MutableDatabaseBackedStoreWithFileIncludes<TModel, TFileModel> modelStore;

        protected DownloadableArchiveModelManager(Storage storage, IDatabaseContextFactory contextFactory, IAPIProvider api, MutableDatabaseBackedStoreWithFileIncludes<TModel, TFileModel> modelStore, IIpcHost importHost = null)
            : base(storage, contextFactory, modelStore, importHost)
        {
            this.api = api;
            this.modelStore = modelStore;
        }

        /// <summary>
        /// Creates the download request for this <see cref="TModel"/>.
        /// </summary>
        /// <param name="model">The <see cref="TModel"/> to be downloaded.</param>
        /// <param name="minimiseDownloadSize">Whether this download should be optimised for slow connections. Generally means extras are not included in the download bundle..</param>
        /// <returns>The request object.</returns>
        protected abstract ArchiveDownloadRequest<TModel> CreateDownloadRequest(TModel model, bool minimiseDownloadSize);

        /// <summary>
        /// Begin a download for the requested <see cref="TModel"/>.
        /// </summary>
        /// <param name="model">The <see cref="TModel"/> to be downloaded.</param>
        /// <param name="minimiseDownloadSize">Whether this download should be optimised for slow connections. Generally means extras are not included in the download bundle..</param>
        /// <returns>Whether the download was started.</returns>
        public bool Download(TModel model, bool minimiseDownloadSize = false)
        {
            if (!canDownload(model)) return false;

            var request = CreateDownloadRequest(model, minimiseDownloadSize);

            performDownloadWithRequest(request);

            return true;
        }

        public virtual bool IsAvailableLocally(TModel model) => modelStore.ConsumableItems.Any(m => m.Equals(model) && !m.DeletePending);

        public ArchiveDownloadRequest<TModel> GetExistingDownload(TModel model) => currentDownloads.Find(r => r.Model.Equals(model));

        private bool canDownload(TModel model) => GetExistingDownload(model) == null && api != null;

        private void performDownloadWithRequest(ArchiveDownloadRequest<TModel> request)
        {
            DownloadNotification notification = new DownloadNotification
            {
                Text = $"Downloading {request.Model}",
            };

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
                    await Import(notification, filename);
                    currentDownloads.Remove(request);
                }, TaskCreationOptions.LongRunning);
            };

            request.Failure += error => handleRequestFailure(request, notification, error);

            notification.CancelRequested += () =>
            {
                request.Cancel();
                currentDownloads.Remove(request);
                notification.State = ProgressNotificationState.Cancelled;
                return true;
            };

            currentDownloads.Add(request);
            PostNotification?.Invoke(notification);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    request.Perform(api);
                }
                catch (Exception e)
                {
                    // 404s (and maybe other failures) don't fire request.Failure so for now they handled here as well
                    handleRequestFailure(request, notification, e);
                }
            }, TaskCreationOptions.LongRunning);

            DownloadBegan?.Invoke(request);
        }

        private void handleRequestFailure(ArchiveDownloadRequest<TModel> req, ProgressNotification notification, Exception e)
        {
            DownloadFailed?.Invoke(req);

            if (e is OperationCanceledException) return;

            notification.State = ProgressNotificationState.Cancelled;
            Logger.Error(e, $"{HumanisedModelName.Titleize()} download failed!");
            currentDownloads.Remove(req);
        }

        private class DownloadNotification : ProgressNotification
        {
            public override bool IsImportant => false;

            protected override Notification CreateCompletionNotification() => new SilencedProgressCompletionNotification
            {
                Activated = CompletionClickAction,
                Text = CompletionText
            };

            private class SilencedProgressCompletionNotification : ProgressCompletionNotification
            {
                public override bool IsImportant => false;
            }
        }
    }
}
