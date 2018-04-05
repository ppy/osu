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

        private readonly Container waveformContainer;
        private readonly BeatmapWaveformGraph waveform;

        public ScrollingTimelineContainer()
            : base(Direction.Horizontal)
        {
            Masking = true;

            Child = waveformContainer = new Container
            {
                RelativeSizeAxes = Axes.Y,
                Child = waveform = new BeatmapWaveformGraph
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex("222"),
                    Depth = float.MaxValue
                }
            };

            waveform.Beatmap.BindTo(Beatmap);
            
            WaveformVisible.ValueChanged += visible => waveform.FadeTo(visible ? 1 : 0, 200, Easing.OutQuint);
        }

        private float zoom = 10;

        protected override void Update()
        {
            base.Update();

            waveformContainer.Margin = new MarginPadding { Horizontal = DrawWidth / 2 };
            waveformContainer.Width = DrawWidth * zoom;
        }
    }
}
