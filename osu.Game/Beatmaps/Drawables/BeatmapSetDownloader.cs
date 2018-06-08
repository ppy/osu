// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;

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
        /// Whether the associated beatmap set has been downloading (by this instance or any other instance).
        /// </summary>
        public readonly BindableBool Downloaded = new BindableBool();

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

            // initial value
            if (set.OnlineBeatmapSetID != null)
                Downloaded.Value = beatmaps.QueryBeatmapSets(s => s.OnlineBeatmapSetID == set.OnlineBeatmapSetID && !s.DeletePending).Any();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmaps != null)
            {
                beatmaps.ItemAdded -= setAdded;
                beatmaps.ItemRemoved -= setRemoved;
            }
        }

        /// <summary>
        /// Begin downloading the associated beatmap set.
        /// </summary>
        /// <returns>True if downloading began. False if an existing download is active or completed.</returns>
        public bool Download()
        {
            if (Downloaded.Value)
                return false;

            if (beatmaps.GetExistingDownload(set) != null)
                return false;

            beatmaps.Download(set, noVideo);
            return true;
        }

        private void setAdded(BeatmapSetInfo s)
        {
            if (s.OnlineBeatmapSetID == set.OnlineBeatmapSetID)
                Downloaded.Value = true;
        }

        private void setRemoved(BeatmapSetInfo s)
        {
            if (s.OnlineBeatmapSetID == set.OnlineBeatmapSetID)
                Downloaded.Value = false;
        }
    }
}
