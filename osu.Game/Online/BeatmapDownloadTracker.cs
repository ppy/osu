// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;

#nullable enable

namespace osu.Game.Online
{
    public class BeatmapDownloadTracker : DownloadTracker<IBeatmapSetInfo>
    {
        [Resolved(CanBeNull = true)]
        protected BeatmapModelDownloader? Downloader { get; private set; }

        private ArchiveDownloadRequest<IBeatmapSetInfo>? attachedRequest;

        private IDisposable? realmSubscription;

        [Resolved]
        private RealmContextFactory realmContextFactory { get; set; } = null!;

        public BeatmapDownloadTracker(IBeatmapSetInfo trackedItem)
            : base(trackedItem)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (Downloader == null)
                return;

            Downloader.DownloadBegan += downloadBegan;
            Downloader.DownloadFailed += downloadFailed;

            // Used to interact with manager classes that don't support interface types. Will eventually be replaced.
            var beatmapSetInfo = new BeatmapSetInfo { OnlineID = TrackedItem.OnlineID };

            realmSubscription = realmContextFactory.Context.All<BeatmapSetInfo>().Where(s => s.OnlineID == TrackedItem.OnlineID && !s.DeletePending).QueryAsyncWithNotifications((items, changes, ___) =>
            {
                if (items.Any())
                    Schedule(() => UpdateState(DownloadState.LocallyAvailable));
                else
                {
                    Schedule(() =>
                    {
                        UpdateState(DownloadState.NotDownloaded);
                        attachDownload(Downloader.GetExistingDownload(beatmapSetInfo));
                    });
                }
            });
        }

        private void downloadBegan(ArchiveDownloadRequest<IBeatmapSetInfo> request) => Schedule(() =>
        {
            if (checkEquality(request.Model, TrackedItem))
                attachDownload(request);
        });

        private void downloadFailed(ArchiveDownloadRequest<IBeatmapSetInfo> request) => Schedule(() =>
        {
            if (checkEquality(request.Model, TrackedItem))
                attachDownload(null);
        });

        private void attachDownload(ArchiveDownloadRequest<IBeatmapSetInfo>? request)
        {
            if (attachedRequest != null)
            {
                attachedRequest.Failure -= onRequestFailure;
                attachedRequest.DownloadProgressed -= onRequestProgress;
                attachedRequest.Success -= onRequestSuccess;
            }

            attachedRequest = request;

            if (attachedRequest != null)
            {
                if (attachedRequest.Progress == 1)
                {
                    UpdateProgress(1);
                    UpdateState(DownloadState.Importing);
                }
                else
                {
                    UpdateProgress(attachedRequest.Progress);
                    UpdateState(DownloadState.Downloading);

                    attachedRequest.Failure += onRequestFailure;
                    attachedRequest.DownloadProgressed += onRequestProgress;
                    attachedRequest.Success += onRequestSuccess;
                }
            }
            else
            {
                UpdateState(DownloadState.NotDownloaded);
            }
        }

        private void onRequestSuccess(string _) => Schedule(() => UpdateState(DownloadState.Importing));

        private void onRequestProgress(float progress) => Schedule(() => UpdateProgress(progress));

        private void onRequestFailure(Exception e) => Schedule(() => attachDownload(null));

        private bool checkEquality(IBeatmapSetInfo x, IBeatmapSetInfo y) => x.OnlineID == y.OnlineID;

        #region Disposal

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            attachDownload(null);

            realmSubscription?.Dispose();

            if (Downloader != null)
            {
                Downloader.DownloadBegan -= downloadBegan;
                Downloader.DownloadFailed -= downloadFailed;
            }
        }

        #endregion
    }
}
