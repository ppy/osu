// Copyright (c) 2007-2019 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests;

namespace osu.Game.Overlays.Direct
{
    public abstract class DownloadTrackingComponent : CompositeDrawable
    {
        private readonly BeatmapSetInfo setInfo;
        private BeatmapManager beatmaps;

        protected DownloadTrackingComponent(BeatmapSetInfo beatmapSetInfo)
        {
            setInfo = beatmapSetInfo;
        }

        #region Disposal

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            beatmaps.BeatmapDownloadBegan -= attachDownload;
            beatmaps.ItemAdded -= setAdded;
        }

        #endregion

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapManager beatmaps)
        {
            this.beatmaps = beatmaps;

            var downloadRequest = beatmaps.GetExistingDownload(setInfo);

            if (downloadRequest != null)
                attachDownload(downloadRequest);

            beatmaps.BeatmapDownloadBegan += attachDownload;
            beatmaps.ItemAdded += setAdded;
        }

        private void attachDownload(DownloadBeatmapSetRequest request)
        {
            if (request.BeatmapSet.OnlineBeatmapSetID != setInfo.OnlineBeatmapSetID)
                return;

            DownloadStarted();

            request.Failure += e => { DownloadFailed(); };

            request.DownloadProgressed += progress => Schedule(() => ProgressChanged(progress));
            request.Success += data => { DownloadComleted(); };
        }

        protected abstract void ProgressChanged(float progress);

        protected abstract void DownloadFailed();

        protected abstract void DownloadComleted();

        protected abstract void BeatmapImported();

        protected abstract void DownloadStarted();

        private void setAdded(BeatmapSetInfo s, bool existing, bool silent)
        {
            if (s.OnlineBeatmapSetID != setInfo.OnlineBeatmapSetID)
                return;

            Schedule(BeatmapImported);
        }
    }
}