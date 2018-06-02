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
        private readonly OverlayContainer owner;

        private readonly BeatmapSetInfo beatmapSetInfo;

        public event Action Stopped;
        public event Action Started;

        public PreviewTrack(BeatmapSetInfo beatmapSetInfo, OverlayContainer owner)
        {
            this.beatmapSetInfo = beatmapSetInfo;
            this.owner = owner;
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

        /// <summary>
        /// Stop preview playback
        /// </summary>
        /// <param name="source">An <see cref="OverlayContainer"/> which is probably the owner of this <see cref="PreviewTrack"/></param>
        public void Stop(OverlayContainer source = null)
        {
            if (source != null && owner != source)
                return;
            Track.Stop();
            Stopped?.Invoke();
        }
    }
}
