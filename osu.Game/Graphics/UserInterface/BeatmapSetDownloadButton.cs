// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;

namespace osu.Game.Graphics.UserInterface
{
    public abstract class BeatmapSetDownloadButton : OsuClickableContainer
    {
        private readonly BeatmapSetInfo set;
        private readonly bool noVideo;

        private BeatmapManager beatmaps;

        protected readonly BindableBool Downloaded = new BindableBool();

        protected BeatmapSetDownloadButton(BeatmapSetInfo set, bool noVideo = false)
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
            Downloaded.Value = beatmaps.QueryBeatmapSets(s => s.OnlineBeatmapSetID == set.OnlineBeatmapSetID && !s.DeletePending).Count() != 0;

            Action = () =>
            {
                if (beatmaps.GetExistingDownload(set) != null)
                {
                    AlreadyDownloading();
                    return;
                }

                beatmaps.Download(set, noVideo);
            };
        }

        protected override bool OnClick(InputState state)
        {
            if (Enabled.Value && !Downloaded.Value)
                Action?.Invoke();
            return true;
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

        protected virtual void AlreadyDownloading()
        {
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
