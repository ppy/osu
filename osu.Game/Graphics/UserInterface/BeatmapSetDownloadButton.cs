// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;

namespace osu.Game.Graphics.UserInterface
{
    public abstract class BeatmapSetDownloadButton : OsuClickableContainer
    {
        private readonly BeatmapSetInfo set;
        private readonly bool noVideo;
        private readonly BindableBool downloaded = new BindableBool();

        private Action action;
        public Action Action
        {
            get => action;
            set => action = value;
        }

        protected BeatmapSetDownloadButton(BeatmapSetInfo set, bool noVideo = false)
        {
            this.set = set;
            this.noVideo = noVideo;

            downloaded.ValueChanged += e =>
            {
                if (e)
                    Disable();
                else
                    Enable();
            };
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmaps, APIAccess api)
        {
            beatmaps.ItemAdded += s =>
            {
                if (s.OnlineBeatmapSetID == set.OnlineBeatmapSetID)
                    downloaded.Value = true;
            };

            beatmaps.ItemRemoved += s =>
            {
                if (s.OnlineBeatmapSetID == set.OnlineBeatmapSetID)
                    downloaded.Value = false;
            };

            // initial downloaded value
            downloaded.Value = beatmaps.QueryBeatmapSets(s => s.OnlineBeatmapSetID == set.OnlineBeatmapSetID && !s.DeletePending).Count() != 0;

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
            if (!downloaded.Value)
                Action?.Invoke();
            return true;
        }

        protected abstract void Enable();
        protected abstract void Disable();
        protected abstract void AlreadyDownloading();
    }
}
