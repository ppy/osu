// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Online.API;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Database
{
    public abstract class ModelDownloader<TModel> : IModelDownloader<TModel>
        where TModel : class, IHasPrimaryKey, ISoftDelete, IEquatable<TModel>
    {
        public Action<Notification> PostNotification { protected get; set; }

        public Action<ArchiveDownloadRequest<TModel>> DownloadBegan { get; set; }

        public Action<ArchiveDownloadRequest<TModel>> DownloadFailed { get; set; }

        private readonly IModelManager<TModel> modelManager;
        private readonly IAPIProvider api;

        protected readonly List<ArchiveDownloadRequest<TModel>> CurrentDownloads = new List<ArchiveDownloadRequest<TModel>>();

        protected ModelDownloader(IModelManager<TModel> modelManager, IAPIProvider api, IIpcHost importHost = null)
        {
            this.modelManager = modelManager;
            this.api = api;
        }

        /// <summary>
        /// Creates the download request for this <typeparamref name="TModel"/>.
        /// </summary>
        /// <param name="model">The <typeparamref name="TModel"/> to be downloaded.</param>
        /// <param name="minimiseDownloadSize">Whether this download should be optimised for slow connections. Generally means extras are not included in the download bundle.</param>
        /// <returns>The request object.</returns>
        protected abstract ArchiveDownloadRequest<TModel> CreateDownloadRequest(TModel model, bool minimiseDownloadSize);

        public bool Download(TModel model, bool minimiseDownloadSize = false)
        {
            if (!canDownload(model)) return false;

            var request = CreateDownloadRequest(model, minimiseDownloadSize);

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
                    var imported = await modelManager.Import(notification, new ImportTask(filename)).ConfigureAwait(false);

                    // for now a failed import will be marked as a failed download for simplicity.
                    if (!imported.Any())
                        DownloadFailed?.Invoke(request);

                    CurrentDownloads.Remove(request);
                }, TaskCreationOptions.LongRunning);
            };

            request.Failure += triggerFailure;

            notification.CancelRequested += () =>
            {
                request.Cancel();
                return true;
            };

            CurrentDownloads.Add(request);
            PostNotification?.Invoke(notification);

            api.PerformAsync(request);

            DownloadBegan?.Invoke(request);
            return true;

            void triggerFailure(Exception error)
            {
                CurrentDownloads.Remove(request);

                DownloadFailed?.Invoke(request);

                notification.State = ProgressNotificationState.Cancelled;

                if (!(error is OperationCanceledException))
                    Logger.Error(error, $"{modelManager.HumanisedModelName.Titleize()} download failed!");
            }
        }

        public abstract ArchiveDownloadRequest<TModel> GetExistingDownload(TModel model);

        private bool canDownload(TModel model) => GetExistingDownload(model) == null && api != null;

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
