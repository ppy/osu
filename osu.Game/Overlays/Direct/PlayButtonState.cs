// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Beatmaps;

namespace osu.Game.Overlays.Direct
{
    public class PlayButtonState : CompositeDrawable
    {
        public BeatmapSetInfo BeatmapSet { get; }
        public PreviewTrack Preview { get; set; }
        public BindableBool Playing { get; }
        public BindableBool Loading { get; }

        private PreviewTrackManager previewTrackManager;

        public PlayButtonState(BeatmapSetInfo beatmapSet)
        {
            BeatmapSet = beatmapSet;
            Playing = new BindableBool();
            Playing.ValueChanged += playingStateChanged;
            Loading = new BindableBool();
        }

        [BackgroundDependencyLoader]
        private void load(PreviewTrackManager previewTrackManager)
        {
            this.previewTrackManager = previewTrackManager;
        }

        private void playingStateChanged(bool playing)
        {
            if (playing)
            {
                if (Preview != null)
                {
                    Preview.Start();
                    return;
                }

                Loading.Value = true;

                LoadComponentAsync(Preview = previewTrackManager.Get(BeatmapSet), preview =>
                {
                    AddInternal(preview);
                    Loading.Value = false;
                    preview.Stopped += () => Playing.Value = false;

                    // user may have changed their mind.
                    if (Playing)
                        preview.Start();
                });
            }
            else
            {
                Preview?.Stop();
                Loading.Value = false;
            }
        }
    }
}
