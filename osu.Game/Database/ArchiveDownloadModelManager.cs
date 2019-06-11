// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Online.API;
using osu.Game.Overlays.Notifications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace osu.Game.Database
{
    /// <summary>
    /// An <see cref="ArchiveModelManager{TModel, TFileModel}"/> that has the ability to download models using an <see cref="IAPIProvider"/> and
    /// import them into the store.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TFileModel">The associated file join type.</typeparam>
    /// <typeparam name="TDownloadRequestModel">The associated <see cref="ArchiveDownloadModelRequest{TModel}"/> for this model.</typeparam>
    public abstract class ArchiveDownloadModelManager<TModel, TFileModel, TDownloadRequestModel> : ArchiveModelManager<TModel, TFileModel>
        where TModel : class, IHasFiles<TFileModel>, IHasPrimaryKey, ISoftDelete
        where TFileModel : INamedFileInfo, new()
        where TDownloadRequestModel : ArchiveDownloadModelRequest<TModel>
    {
        /// <summary>
        /// Fired when a <see cref="TModel"/> download begins.
        /// </summary>
        public event Action<TDownloadRequestModel> DownloadBegan;

        /// <summary>
        /// Fired when a <see cref="TModel"/> download is interrupted, either due to user cancellation or failure.
        /// </summary>
        public event Action<TDownloadRequestModel> DownloadFailed;

        private readonly IAPIProvider api;

        private readonly List<TDownloadRequestModel> currentDownloads = new List<TDownloadRequestModel>();

        protected ArchiveDownloadModelManager(Storage storage, IDatabaseContextFactory contextFactory, IAPIProvider api, MutableDatabaseBackedStoreWithFileIncludes<TModel, TFileModel> modelStore, IIpcHost importHost = null)
            :base(storage, contextFactory, modelStore, importHost)
        {
            this.api = api;
        }

        /// <summary>
        /// Creates the download request for this <see cref="TModel"/>.
        /// The <paramref name="options"/> parameters will be provided when the download was initiated with extra options meant
        /// to be used in the creation of the request.
        /// </summary>
        /// <param name="model">The <see cref="TModel"/> to be downloaded.</param>
        /// <param name="options">Extra parameters for request creation, null if none were passed.</param>
        /// <returns>The request object.</returns>
        protected abstract TDownloadRequestModel CreateDownloadRequest(TModel model, object[] options);

        /// <summary>
        /// Downloads a <see cref="TModel"/>.
        /// This will post notifications tracking progress.
        /// </summary>
        /// <param name="model">The <see cref="TModel"/> to be downloaded.</param>
        /// <returns>Whether downloading can happen.</returns>
        public bool Download(TModel model)
        {
            if (!canDownload(model)) return false;

            var request = CreateDownloadRequest(model, null);

            performDownloadWithRequest(request);

            return true;
        }

        /// <summary>
        /// Downloads a <see cref="TModel"/> with optional parameters for the download request.
        /// </summary>
        /// <param name="model">The <see cref="TModel"/> to be downloaded.</param>
        /// <param name="extra">Optional parameters to be used for creating the download request.</param>
        /// <returns>Whether downloading can happen.</returns>
        public bool Download(TModel model, params object[] extra)
        {
            if (!canDownload(model)) return false;

            var request = CreateDownloadRequest(model, extra);

            performDownloadWithRequest(request);

            return true;
        }

        /// <summary>
        /// Gets an existing <see cref="TModel"/> download request if it exists.
        /// </summary>
        /// <param name="model">The <see cref="TModel"/> whose request is wanted.</param>
        /// <returns>The <see cref="TDownloadRequestModel"/> object if it exists, otherwise null.</returns>
        public TDownloadRequestModel GetExistingDownload(TModel model) => currentDownloads.Find(r => r.Info.Equals(model));

        private bool canDownload(TModel model) => GetExistingDownload(model) == null && api != null;

        private void performDownloadWithRequest(TDownloadRequestModel request)
        {
            DownloadNotification notification = new DownloadNotification
            {
                Text = $"Downloading {request.Info}",
            };

            request.DownloadProgressed += progress =>
            {
                notification.State = ProgressNotificationState.Active;
                notification.Progress = progress;
            };

            request.Success += filename =>
            {
                Task.Factory.StartNew(() =>
                {
                    Import(notification, filename);
                    currentDownloads.Remove(request);
                }, TaskCreationOptions.LongRunning);
            };

            request.Failure += error =>
            {
                DownloadFailed?.Invoke(request);

                if (error is OperationCanceledException) return;

                notification.State = ProgressNotificationState.Cancelled;
                // TODO: maybe implement a Name for every model that we can use in this message?
                Logger.Error(error, "Download failed!");
                currentDownloads.Remove(request);
            };

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
                catch
                {
                }
            }, TaskCreationOptions.LongRunning);

            DownloadBegan?.Invoke(request);
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
