// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.API;

#nullable enable

namespace osu.Game.Online
{
    public class BeatmapDownloadTracker : DownloadTracker<IBeatmapSetInfo>
    {
        [Resolved(CanBeNull = true)]
        protected BeatmapManager? Manager { get; private set; }

        private ArchiveDownloadRequest<IBeatmapSetInfo>? attachedRequest;

        public BeatmapDownloadTracker(IBeatmapSetInfo trackedItem)
            : base(trackedItem)
        {
        }

        [BackgroundDependencyLoader(true)]
        private void load()
        {
            if (Manager == null)
                return;

            // Used to interact with manager classes that don't support interface types. Will eventually be replaced.
            var beatmapSetInfo = new BeatmapSetInfo { OnlineID = TrackedItem.OnlineID };

            if (Manager.IsAvailableLocally(beatmapSetInfo))
                UpdateState(DownloadState.LocallyAvailable);
            else
                attachDownload(Manager.GetExistingDownload(beatmapSetInfo));

            Manager.DownloadBegan += downloadBegan;
            Manager.DownloadFailed += downloadFailed;
            Manager.ItemUpdated += itemUpdated;
            Manager.ItemRemoved += itemRemoved;
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

        private void itemUpdated(BeatmapSetInfo item) => Schedule(() =>
        {
            if (checkEquality(item, TrackedItem))
                UpdateState(DownloadState.LocallyAvailable);
        });

        private void itemRemoved(BeatmapSetInfo item) => Schedule(() =>
        {
            if (checkEquality(item, TrackedItem))
                UpdateState(DownloadState.NotDownloaded);
        });

        private bool checkEquality(IBeatmapSetInfo x, IBeatmapSetInfo y) => x.OnlineID == y.OnlineID;

        #region Disposal

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            attachDownload(null);

            if (Manager != null)
            {
                Manager.DownloadBegan -= downloadBegan;
                Manager.DownloadFailed -= downloadFailed;
                Manager.ItemUpdated -= itemUpdated;
                Manager.ItemRemoved -= itemRemoved;
            }
        }

        #endregion
    }
}
