// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Database
{
    public abstract class ModelDownloader<TModel, T> : IModelDownloader<T>
        where TModel : class, IHasPrimaryKey, ISoftDelete, IEquatable<TModel>, T
        where T : class
    {
        public Action<Notification> PostNotification { protected get; set; }

        public event Action<ArchiveDownloadRequest<T>> DownloadBegan;

        public event Action<ArchiveDownloadRequest<T>> DownloadFailed;

        private readonly IModelImporter<TModel> importer;
        private readonly IAPIProvider api;

        protected readonly List<ArchiveDownloadRequest<T>> CurrentDownloads = new List<ArchiveDownloadRequest<T>>();

        protected ModelDownloader(IModelImporter<TModel> importer, IAPIProvider api, IIpcHost importHost = null)
        {
            this.importer = importer;
            this.api = api;
        }

        /// <summary>
        /// Creates the download request for this <typeparamref name="T"/>.
        /// </summary>
        /// <param name="model">The <typeparamref name="T"/> to be downloaded.</param>
        /// <param name="isMini">Whether this downlaod should be optimised for very slow connections. Generally means any extra files are not included in the download bundle.</param>
        /// <returns>The request object.</returns>
        protected abstract ArchiveDownloadRequest<T> CreateDownloadRequest(T model, bool isMini);

        protected abstract ArchiveDownloadRequest<T> CreateAccelDownloadRequest(T model, bool isMini);

        private bool accel;

        /// <summary>
        /// Begin a download for the requested <typeparamref name="T"/>.
        /// </summary>
        /// <param name="model">The <typeparamref name="T"/> to be downloaded.</param>
        /// <param name="minimiseDownloadSize">Upstream arg</param>
        /// <returns>Whether the download was started.</returns>
        public bool Download(T model, bool minimiseDownloadSize)
        {
            if (!canDownload(model)) return false;

            var request = accel
                ? CreateAccelDownloadRequest(model, minimiseDownloadSize)
                : CreateDownloadRequest(model, minimiseDownloadSize);

            accel = false;

            DownloadNotification notification = new DownloadNotification
            {
                Text = $"正在下载 {request.Model.GetDisplayString()}",
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
                    var imported = await importer.Import(notification, new ImportTask(filename)).ConfigureAwait(false);

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
                    Logger.Error(error, $"{importer.HumanisedModelName.Titleize()} 下载失败!");
            }
        }

        public bool AccelDownload(T model, bool minimiseDownloadSize)
        {
            accel = true;
            return Download(model, minimiseDownloadSize);
        }

        public abstract ArchiveDownloadRequest<T> GetExistingDownload(T model);

        private bool canDownload(T model) => GetExistingDownload(model) == null && api != null;

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
