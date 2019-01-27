// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests;

namespace osu.Game.Beatmaps.Drawables
{
    /// <summary>
    /// A component to allow downloading of a beatmap set. Automatically handles state syncing between other instances.
    /// </summary>
    public class BeatmapSetDownloader : Component
    {
        private readonly BeatmapSetInfo set;
        private readonly bool noVideo;

        private BeatmapManager beatmaps;

        /// <summary>
        /// Holds the current download state of the beatmap, whether is has already been downloaded, is in progress, or is not downloaded.
        /// </summary>
        public readonly Bindable<DownloadStatus> DownloadState = new Bindable<DownloadStatus>();

        public BeatmapSetDownloader(BeatmapSetInfo set, bool noVideo = false)
        {
            this.set = set;
            this.noVideo = noVideo;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmaps)
        {
            this.beatmaps = beatmaps;

            beatmaps.ItemAdded += setAdded;
            beatmaps.ItemRemoved += setRemoved;
            beatmaps.BeatmapDownloadBegan += downloadBegan;
            beatmaps.BeatmapDownloadFailed += downloadFailed;

            // initial value
            if (set.OnlineBeatmapSetID != null && beatmaps.QueryBeatmapSets(s => s.OnlineBeatmapSetID == set.OnlineBeatmapSetID && !s.DeletePending).Any())
                DownloadState.Value = DownloadStatus.Downloaded;
            else if (beatmaps.GetExistingDownload(set) != null)
                DownloadState.Value = DownloadStatus.Downloading;
            else
                DownloadState.Value = DownloadStatus.NotDownloaded;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmaps != null)
            {
                beatmaps.ItemAdded -= setAdded;
                beatmaps.ItemRemoved -= setRemoved;
                beatmaps.BeatmapDownloadBegan -= downloadBegan;
                beatmaps.BeatmapDownloadFailed -= downloadFailed;
            }
        }

        /// <summary>
        /// Begin downloading the associated beatmap set.
        /// </summary>
        /// <returns>True if downloading began. False if an existing download is active or completed.</returns>
        public void Download()
        {
            if (DownloadState.Value > DownloadStatus.NotDownloaded)
                return;

            if (beatmaps.Download(set, noVideo))
            {
                // Only change state if download can happen
                DownloadState.Value = DownloadStatus.Downloading;
            }
        }

        private void setAdded(BeatmapSetInfo s, bool existing, bool silent) => Schedule(() =>
        {
            if (s.OnlineBeatmapSetID == set.OnlineBeatmapSetID)
                DownloadState.Value = DownloadStatus.Downloaded;
        });

        private void setRemoved(BeatmapSetInfo s) => Schedule(() =>
        {
            if (s.OnlineBeatmapSetID == set.OnlineBeatmapSetID)
                DownloadState.Value = DownloadStatus.NotDownloaded;
        });

        private void downloadBegan(DownloadBeatmapSetRequest d)
        {
            if (d.BeatmapSet.OnlineBeatmapSetID == set.OnlineBeatmapSetID)
                DownloadState.Value = DownloadStatus.Downloading;
        }

        private void downloadFailed(DownloadBeatmapSetRequest d)
        {
            if (d.BeatmapSet.OnlineBeatmapSetID == set.OnlineBeatmapSetID)
                DownloadState.Value = DownloadStatus.NotDownloaded;
        }

        public enum DownloadStatus
        {
            NotDownloaded,
            Downloading,
            Downloaded,
        }
    }
}
