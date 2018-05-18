// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Screens.Compose.Timeline
{
    public class Timeline : ZoomableScrollContainer
    {
        public readonly Bindable<bool> WaveformVisible = new Bindable<bool>();
        public readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        public Timeline()
        {
            BeatmapWaveformGraph waveform;
            Child = waveform = new BeatmapWaveformGraph
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.FromHex("222"),
                Depth = float.MaxValue
            };

            waveform.Beatmap.BindTo(Beatmap);

            WaveformVisible.ValueChanged += visible => waveform.FadeTo(visible ? 1 : 0, 200, Easing.OutQuint);
        }

        protected override void Update()
        {
            base.Update();

            // We want time = 0 to be at the centre of the container when scrolled to the start
            Content.Margin = new MarginPadding { Horizontal = DrawWidth / 2 };
        }
    }
}
