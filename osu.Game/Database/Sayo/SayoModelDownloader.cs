// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Online.API;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Database.Sayo
{
    /// <summary>
    /// 从<see cref="ModelDownloader{TModel}"/>修改而来
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class SayoModelDownloader<TModel> : ISayoModelDownloader<TModel>
        where TModel : class, IHasPrimaryKey, ISoftDelete, IEquatable<TModel>
    {
        public Action<Notification> PostNotification { protected get; set; }

        public IBindable<WeakReference<ArchiveDownloadRequest<TModel>>> SayoDownloadBegan => downloadBegan;

        private readonly Bindable<WeakReference<ArchiveDownloadRequest<TModel>>> downloadBegan = new Bindable<WeakReference<ArchiveDownloadRequest<TModel>>>();

        public IBindable<WeakReference<ArchiveDownloadRequest<TModel>>> SayoDownloadFailed => downloadFailed;

        private readonly Bindable<WeakReference<ArchiveDownloadRequest<TModel>>> downloadFailed = new Bindable<WeakReference<ArchiveDownloadRequest<TModel>>>();

        private readonly IModelManager<TModel> modelManager;
        private readonly IAPIProvider api;

        private readonly List<ArchiveDownloadRequest<TModel>> currentDownloads = new List<ArchiveDownloadRequest<TModel>>();

        protected SayoModelDownloader(IModelManager<TModel> modelManager, IAPIProvider api, IIpcHost importHost = null)
        {
            this.modelManager = modelManager;
            this.api = api;
        }

        /// <summary>
        /// Creates the download request for this <typeparamref name="TModel"/>.
        /// </summary>
        /// <param name="model">The <typeparamref name="TModel"/> to be downloaded.</param>
        /// <param name="noVideo">不带视频</param>
        /// <param name="isMini">Whether this downlaod should be optimised for very slow connections. Generally means any extra files are not included in the download bundle.</param>
        /// <returns>The request object.</returns>
        protected abstract ArchiveDownloadRequest<TModel> CreateSayoDownloadRequest(TModel model, bool noVideo, bool isMini);

        /// <summary>
        /// 从sayobot下载请求的 <typeparamref name="TModel"/>.
        /// </summary>
        /// <param name="model">目标 <typeparamref name="TModel"/></param>
        /// <param name="noVideo">不带视频</param>
        /// <param name="mini">Mini</param>
        /// <returns>下载是否开始</returns>
        /// <remarks>noVideo和mini不能同时为true</remarks>
        public bool SayoDownload(TModel model, bool noVideo, bool mini)
        {
            if (!canDownload(model)) return false;

            if (noVideo && mini) return false;

            var request = CreateSayoDownloadRequest(model, noVideo, mini);

            DownloadNotification notification = new DownloadNotification
            {
                Text = $"正在下载 {request.Model}"
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
                        downloadFailed.Value = new WeakReference<ArchiveDownloadRequest<TModel>>(request);

                    currentDownloads.Remove(request);
                }, TaskCreationOptions.LongRunning);
            };

            request.Failure += triggerFailure;

            notification.CancelRequested += () =>
            {
                request.Cancel();
                return true;
            };

            currentDownloads.Add(request);
            PostNotification?.Invoke(notification);

            api.PerformAsync(request);

            downloadBegan.Value = new WeakReference<ArchiveDownloadRequest<TModel>>(request);
            return true;

            void triggerFailure(Exception error)
            {
                currentDownloads.Remove(request);

                downloadFailed.Value = new WeakReference<ArchiveDownloadRequest<TModel>>(request);

                downloadBegan.Value = new WeakReference<ArchiveDownloadRequest<TModel>>(request);

                notification.State = ProgressNotificationState.Cancelled;

                if (!(error is OperationCanceledException))
                    Logger.Error(error, $"{modelManager.HumanisedModelName.Titleize()}下载失败!");
            }
        }

        public ArchiveDownloadRequest<TModel> GetExistingDownload(TModel model) => currentDownloads.Find(r => r.Model.Equals(model));

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
