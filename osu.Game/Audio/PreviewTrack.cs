// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;

namespace osu.Game.Audio
{
    public class PreviewTrack : Component
    {
        public Track Track { get; private set; }
        public readonly OverlayContainer Owner;

        private readonly BeatmapSetInfo beatmapSetInfo;

        public event Action Stopped;
        public event Action Started;

        public PreviewTrack(BeatmapSetInfo beatmapSetInfo, OverlayContainer owner)
        {
            this.beatmapSetInfo = beatmapSetInfo;
            Owner = owner;
        }

        [BackgroundDependencyLoader]
        private void load(PreviewTrackManager previewTrackManager)
        {
            Track = previewTrackManager.Get(this, beatmapSetInfo);
        }

        public void Start()
        {
            Track.Restart();
            Started?.Invoke();
        }

        public void Stop()
        {
            Track.Stop();
            Stopped?.Invoke();
        }
    }
}
