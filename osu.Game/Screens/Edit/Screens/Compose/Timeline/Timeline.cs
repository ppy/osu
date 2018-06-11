// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Screens.Compose.Timeline
{
    public class Timeline : ZoomableScrollContainer
    {
        public readonly Bindable<bool> WaveformVisible = new Bindable<bool>();
        public readonly IBindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        public Timeline()
        {
            ZoomDuration = 200;
            ZoomEasing = Easing.OutQuint;
            Zoom = 10;
        }

        private WaveformGraph waveform;

        [BackgroundDependencyLoader]
        private void load(IBindableBeatmap beatmap)
        {
            Child = waveform = new WaveformGraph
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.FromHex("222"),
                Depth = float.MaxValue
            };

            WaveformVisible.ValueChanged += visible => waveform.FadeTo(visible ? 1 : 0, 200, Easing.OutQuint);

            Beatmap.BindTo(beatmap);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Beatmap.BindValueChanged(b => waveform.Waveform = b.Waveform);
            waveform.Waveform = Beatmap.Value.Waveform;
        }

        protected override void Update()
        {
            base.Update();

            // We want time = 0 to be at the centre of the container when scrolled to the start
            Content.Margin = new MarginPadding { Horizontal = DrawWidth / 2 };
        }
    }
}
