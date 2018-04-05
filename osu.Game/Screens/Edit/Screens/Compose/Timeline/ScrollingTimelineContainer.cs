// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Screens.Compose.Timeline
{
    public class ScrollingTimelineContainer : ScrollContainer
    {
        public readonly Bindable<bool> WaveformVisible = new Bindable<bool>();
        public readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        private readonly BeatmapWaveformGraph waveform;

        public ScrollingTimelineContainer()
            : base(Direction.Horizontal)
        {
            Masking = true;

            Content.AutoSizeAxes = Axes.None;
            Content.RelativeSizeAxes = Axes.Both;

            Add(waveform = new BeatmapWaveformGraph
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.FromHex("222"),
                Depth = float.MaxValue
            });

            waveform.Beatmap.BindTo(Beatmap);
            WaveformVisible.ValueChanged += waveformVisibilityChanged;
        }

        private void waveformVisibilityChanged(bool visible) => waveform.FadeTo(visible ? 1 : 0, 200, Easing.OutQuint);
    }
}
