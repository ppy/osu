// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.API;
using osu.Game.Scoring;

#nullable enable

namespace osu.Game.Online
{
    public class ScoreDownloadTracker : DownloadTracker<ScoreInfo>
    {
        [Resolved(CanBeNull = true)]
        protected ScoreManager? Manager { get; private set; }

        private ArchiveDownloadRequest<IScoreInfo>? attachedRequest;

        public ScoreDownloadTracker(ScoreInfo trackedItem)
            : base(trackedItem)
        {
        }

        private IBindable<WeakReference<ScoreInfo>>? managerUpdated;
        private IBindable<WeakReference<ScoreInfo>>? managerRemoved;
        private IBindable<WeakReference<ArchiveDownloadRequest<IScoreInfo>>>? managerDownloadBegan;
        private IBindable<WeakReference<ArchiveDownloadRequest<IScoreInfo>>>? managerDownloadFailed;

        [BackgroundDependencyLoader(true)]
        private void load()
        {
            if (Manager == null)
                return;

            // Used to interact with manager classes that don't support interface types. Will eventually be replaced.
            var scoreInfo = new ScoreInfo
            {
                ID = TrackedItem.ID,
                OnlineScoreID = TrackedItem.OnlineScoreID
            };

            if (Manager.IsAvailableLocally(scoreInfo))
                UpdateState(DownloadState.LocallyAvailable);
            else
                attachDownload(Manager.GetExistingDownload(scoreInfo));

            managerDownloadBegan = Manager.DownloadBegan.GetBoundCopy();
            managerDownloadBegan.BindValueChanged(downloadBegan);
            managerDownloadFailed = Manager.DownloadFailed.GetBoundCopy();
            managerDownloadFailed.BindValueChanged(downloadFailed);
            managerUpdated = Manager.ItemUpdated.GetBoundCopy();
            managerUpdated.BindValueChanged(itemUpdated);
            managerRemoved = Manager.ItemRemoved.GetBoundCopy();
            managerRemoved.BindValueChanged(itemRemoved);
        }

        private void downloadBegan(ValueChangedEvent<WeakReference<ArchiveDownloadRequest<IScoreInfo>>> weakRequest)
        {
            if (weakRequest.NewValue.TryGetTarget(out var request))
            {
                Schedule(() =>
                {
                    if (checkEquality(request.Model, TrackedItem))
                        attachDownload(request);
                });
            }
        }

        private void downloadFailed(ValueChangedEvent<WeakReference<ArchiveDownloadRequest<IScoreInfo>>> weakRequest)
        {
            if (weakRequest.NewValue.TryGetTarget(out var request))
            {
                Schedule(() =>
                {
                    if (checkEquality(request.Model, TrackedItem))
                        attachDownload(null);
                });
            }
        }

        private void attachDownload(ArchiveDownloadRequest<IScoreInfo>? request)
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

        private void itemUpdated(ValueChangedEvent<WeakReference<ScoreInfo>> weakItem)
        {
            if (weakItem.NewValue.TryGetTarget(out var item))
            {
                Schedule(() =>
                {
                    if (checkEquality(item, TrackedItem))
                        UpdateState(DownloadState.LocallyAvailable);
                });
            }
        }

        private void itemRemoved(ValueChangedEvent<WeakReference<ScoreInfo>> weakItem)
        {
            if (weakItem.NewValue.TryGetTarget(out var item))
            {
                Schedule(() =>
                {
                    if (checkEquality(item, TrackedItem))
                        UpdateState(DownloadState.NotDownloaded);
                });
            }
        }

        private bool checkEquality(IScoreInfo x, IScoreInfo y) => x.OnlineID == y.OnlineID;

        #region Disposal

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            attachDownload(null);
        }

        #endregion
    }
}
