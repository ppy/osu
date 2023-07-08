// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Scoring;

namespace osu.Game.Online
{
    public partial class ScoreDownloadTracker : DownloadTracker<ScoreInfo>
    {
        [Resolved(CanBeNull = true)]
        protected ScoreModelDownloader? Downloader { get; private set; }

        private ArchiveDownloadRequest<IScoreInfo>? attachedRequest;

        private IDisposable? realmSubscription;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        public ScoreDownloadTracker(ScoreInfo trackedItem)
            : base(trackedItem)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (Downloader == null)
                return;

            // Used to interact with manager classes that don't support interface types. Will eventually be replaced.
            var scoreInfo = new ScoreInfo
            {
                ID = TrackedItem.ID,
                OnlineID = TrackedItem.OnlineID
            };

            Downloader.DownloadBegan += downloadBegan;
            Downloader.DownloadFailed += downloadFailed;

            realmSubscription = realm.RegisterForNotifications(r => r.All<ScoreInfo>().Where(s =>
                ((s.OnlineID > 0 && s.OnlineID == TrackedItem.OnlineID)
                 || (!string.IsNullOrEmpty(s.Hash) && s.Hash == TrackedItem.Hash))
                && !s.DeletePending), (items, _) =>
            {
                if (items.Any())
                    Schedule(() => UpdateState(DownloadState.LocallyAvailable));
                else
                {
                    Schedule(() =>
                    {
                        UpdateState(DownloadState.NotDownloaded);
                        attachDownload(Downloader.GetExistingDownload(scoreInfo));
                    });
                }
            });
        }

        private void downloadBegan(ArchiveDownloadRequest<IScoreInfo> request) => Schedule(() =>
        {
            if (checkEquality(request.Model, TrackedItem))
                attachDownload(request);
        });

        private void downloadFailed(ArchiveDownloadRequest<IScoreInfo> request) => Schedule(() =>
        {
            if (checkEquality(request.Model, TrackedItem))
                attachDownload(null);
        });

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

        private bool checkEquality(IScoreInfo x, IScoreInfo y) => x.MatchesOnlineID(y);

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
