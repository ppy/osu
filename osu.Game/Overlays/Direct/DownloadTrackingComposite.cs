// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests;

namespace osu.Game.Overlays.Direct
{
    public abstract class DownloadTrackingComposite : CompositeDrawable
    {
        public readonly Bindable<BeatmapSetInfo> BeatmapSet = new Bindable<BeatmapSetInfo>();

        private BeatmapManager beatmaps;

        /// <summary>
        /// Holds the current download state of the beatmap, whether is has already been downloaded, is in progress, or is not downloaded.
        /// </summary>
        protected readonly Bindable<DownloadState> State = new Bindable<DownloadState>();

        protected readonly Bindable<double> Progress = new Bindable<double>();

        protected DownloadTrackingComposite(BeatmapSetInfo beatmapSet = null)
        {
            BeatmapSet.Value = beatmapSet;
        }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapManager beatmaps)
        {
            this.beatmaps = beatmaps;

            BeatmapSet.BindValueChanged(set =>
            {
                if (set == null)
                    attachDownload(null);
                else if (beatmaps.QueryBeatmapSets(s => s.OnlineBeatmapSetID == set.OnlineBeatmapSetID).Any())
                    State.Value = DownloadState.LocallyAvailable;
                else
                    attachDownload(beatmaps.GetExistingDownload(set));
            }, true);

            beatmaps.BeatmapDownloadBegan += download =>
            {
                if (download.BeatmapSet.OnlineBeatmapSetID == BeatmapSet.Value?.OnlineBeatmapSetID)
                    attachDownload(download);
            };

            beatmaps.ItemAdded += setAdded;
        }

        #region Disposal

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            beatmaps.BeatmapDownloadBegan -= attachDownload;
            beatmaps.ItemAdded -= setAdded;

            State.UnbindAll();

            attachDownload(null);
        }

        #endregion

        private DownloadBeatmapSetRequest attachedRequest;

        private void attachDownload(DownloadBeatmapSetRequest request)
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
                    State.Value = DownloadState.Downloaded;
                    Progress.Value = 1;
                }
                else
                {
                    State.Value = DownloadState.Downloading;
                    Progress.Value = attachedRequest.Progress;

                    attachedRequest.Failure += onRequestFailure;
                    attachedRequest.DownloadProgressed += onRequestProgress;
                    attachedRequest.Success += onRequestSuccess;
                }
            }
            else
            {
                State.Value = DownloadState.NotDownloaded;
            }
        }

        private void onRequestSuccess(string data)
        {
            Schedule(() => State.Value = DownloadState.Downloaded);
        }

        private void onRequestProgress(float progress)
        {
            Schedule(() => Progress.Value = progress);
        }

        private void onRequestFailure(Exception e)
        {
            Schedule(() => attachDownload(null));
        }

        private void setAdded(BeatmapSetInfo s, bool existing, bool silent)
        {
            if (s.OnlineBeatmapSetID != BeatmapSet.Value?.OnlineBeatmapSetID)
                return;

            Schedule(() => State.Value = DownloadState.LocallyAvailable);
        }
    }
}
