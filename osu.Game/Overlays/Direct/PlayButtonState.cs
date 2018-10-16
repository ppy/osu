// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Beatmaps;

namespace osu.Game.Overlays.Direct
{
    public class PlayButtonState : CompositeDrawable, IStateful<PlayButtonState.PlaybackState>
    {
        public PlaybackState State { get; set; }
        public event Action<PlaybackState> StateChanged;

        public BeatmapSetInfo BeatmapSet { get; }

        private WeakReference previewWeakReference;

        public PreviewTrack Preview
        {
            get => (PreviewTrack)(previewWeakReference != null && previewWeakReference.IsAlive ? previewWeakReference.Target : null);
            set => previewWeakReference = new WeakReference(value);
        }

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
            StateChanged?.Invoke(State = playing ? PlaybackState.Playing : PlaybackState.Stopped);
            if (playing)
            {
                if (Preview != null)
                {
                    AddInternal(Preview);
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
                if (Preview != null)
                {
                    RemoveInternal(Preview);
                    Preview.Stop();
                }

                Loading.Value = false;
            }
        }

        public enum PlaybackState
        {
            Playing,
            Stopped
        }
    }
}
