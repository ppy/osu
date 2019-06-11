using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Online.API;
using osu.Game.Overlays.Notifications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace osu.Game.Database
{
    public abstract class ArchiveDownloadModelManager<TModel, TFileModel, TDownloadRequestModel> : ArchiveModelManager<TModel, TFileModel>
        where TModel : class, IHasFiles<TFileModel>, IHasPrimaryKey, ISoftDelete
        where TFileModel : INamedFileInfo, new()
        where TDownloadRequestModel : ArchiveDownloadModelRequest<TModel>
    {
        public event Action<TDownloadRequestModel> DownloadBegan;

        public event Action<TDownloadRequestModel> DownloadFailed;

        private readonly IAPIProvider api;

        private readonly List<TDownloadRequestModel> currentDownloads = new List<TDownloadRequestModel>();

        protected ArchiveDownloadModelManager(Storage storage, IDatabaseContextFactory contextFactory, IAPIProvider api, MutableDatabaseBackedStoreWithFileIncludes<TModel, TFileModel> modelStore, IIpcHost importHost = null)
            :base(storage, contextFactory, modelStore, importHost)
        {
            this.api = api;
        }

        protected abstract TDownloadRequestModel CreateDownloadRequest(TModel model);

        public bool Download(TModel model)
        {
            var existing = GetExistingDownload(model);

            if (existing != null || api == null) return false;

            DownloadNotification notification = new DownloadNotification
            {
                Text = $"Downloading {model}",
            };

            var request = CreateDownloadRequest(model);

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
                // TODO: implement a Name for every model that we can use in this message
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

            return true;
        }

        public TDownloadRequestModel GetExistingDownload(TModel model) => currentDownloads.Find(r => r.Info.Equals(model));

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
