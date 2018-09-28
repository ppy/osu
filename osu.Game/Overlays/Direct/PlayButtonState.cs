// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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

        public PlayButtonState(BeatmapSetInfo beatmapSet)
        {
            BeatmapSet = beatmapSet;
            Playing = new BindableBool();
            Playing.ValueChanged += playingStateChanged;
            Loading = new BindableBool();
        }

        private void playingStateChanged(bool playing)
        {
            throw new NotImplementedException();
        }
    }
}
