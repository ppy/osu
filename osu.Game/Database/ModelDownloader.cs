// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Humanizer;
using osu.Framework.Logging;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Database
{
    public abstract partial class ModelDownloader<TModel, T> : IModelDownloader<T>
        where TModel : class, IHasGuidPrimaryKey, ISoftDelete, IEquatable<TModel>, T
        where T : class
    {
        public Action<Notification>? PostNotification { protected get; set; }

        public event Action<ArchiveDownloadRequest<T>>? DownloadBegan;

        public event Action<ArchiveDownloadRequest<T>>? DownloadFailed;

        private readonly IModelImporter<TModel> importer;
        private readonly IAPIProvider? api;

        protected readonly List<ArchiveDownloadRequest<T>> CurrentDownloads = new List<ArchiveDownloadRequest<T>>();

        protected ModelDownloader(IModelImporter<TModel> importer, IAPIProvider? api)
        {
            this.importer = importer;
            this.api = api;
        }

        /// <summary>
        /// Creates the download request for this <typeparamref name="T"/>.
        /// </summary>
        /// <param name="model">The <typeparamref name="T"/> to be downloaded.</param>
        /// <param name="minimiseDownloadSize">Whether this download should be optimised for slow connections. Generally means extras are not included in the download bundle.</param>
        /// <returns>The request object.</returns>
        protected abstract ArchiveDownloadRequest<T> CreateDownloadRequest(T model, bool minimiseDownloadSize);

        public bool Download(T model, bool minimiseDownloadSize = false) => Download(model, minimiseDownloadSize, null);

        public void DownloadAsUpdate(TModel originalModel, bool minimiseDownloadSize) => Download(originalModel, minimiseDownloadSize, originalModel);

        protected bool Download(T model, bool minimiseDownloadSize, TModel? originalModel)
        {
            if (!canDownload(model)) return false;

            var request = CreateDownloadRequest(model, minimiseDownloadSize);

            DownloadNotification notification = new DownloadNotification
            {
                Text = $"Downloading {request.Model.GetDisplayString()}",
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
                    bool importSuccessful;

                    if (originalModel != null)
                        importSuccessful = (await importer.ImportAsUpdate(notification, new ImportTask(filename), originalModel).ConfigureAwait(false)) != null;
                    else
                        importSuccessful = (await importer.Import(notification, new[] { new ImportTask(filename) }).ConfigureAwait(false)).Any();

                    // for now a failed import will be marked as a failed download for simplicity.
                    if (!importSuccessful)
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

            api?.PerformAsync(request);

            DownloadBegan?.Invoke(request);
            return true;

            void triggerFailure(Exception error)
            {
                CurrentDownloads.Remove(request);

                DownloadFailed?.Invoke(request);

                notification.State = ProgressNotificationState.Cancelled;

                if (!(error is OperationCanceledException))
                {
                    if (error is WebException webException && webException.Message == @"TooManyRequests")
                    {
                        notification.Close(false);
                        PostNotification?.Invoke(new TooManyDownloadsNotification());
                    }
                    else
                        Logger.Error(error, $"{importer.HumanisedModelName.Titleize()} download failed!");
                }
            }
        }

        public abstract ArchiveDownloadRequest<T>? GetExistingDownload(T model);

        private bool canDownload(T model) => GetExistingDownload(model) == null && api != null;

        private partial class DownloadNotification : ProgressNotification
        {
            public override bool IsImportant => false;

            protected override Notification CreateCompletionNotification() => new SilencedProgressCompletionNotification
            {
                Activated = CompletionClickAction,
                Text = CompletionText
            };

            private partial class SilencedProgressCompletionNotification : ProgressCompletionNotification
            {
                public override bool IsImportant => false;
            }
        }
    }
}
