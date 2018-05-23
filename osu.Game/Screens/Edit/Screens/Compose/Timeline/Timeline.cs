// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Screens.Compose.Timeline
{
    public class Timeline : ZoomableScrollContainer
    {
        public readonly Bindable<bool> WaveformVisible = new Bindable<bool>();
        public readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        private IAdjustableClock adjustableClock;

        public Timeline()
        {
            ZoomDuration = 200;
            ZoomEasing = Easing.OutQuint;
            Zoom = 10;

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

        [BackgroundDependencyLoader]
        private void load(IAdjustableClock adjustableClock)
        {
            this.adjustableClock = adjustableClock;
        }

        private bool handlingUserInput;
        private bool trackWasPlaying;

        protected override void Update()
        {
            base.Update();

            // We want time = 0 to be at the centre of the container when scrolled to the start
            Content.Margin = new MarginPadding { Horizontal = DrawWidth / 2 };

            if (!handlingUserInput)
                ScrollTo((float)(adjustableClock.CurrentTime / Beatmap.Value.Track.Length) * Content.DrawWidth, false);
            else
                adjustableClock.Seek(Current / Content.DrawWidth * Beatmap.Value.Track.Length);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            if (base.OnMouseDown(state, args))
            {
                beginUserInput();
                return true;
            }

            return false;
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            endUserInput();
            return base.OnMouseUp(state, args);
        }

        private void beginUserInput()
        {
            handlingUserInput = true;
            trackWasPlaying = adjustableClock.IsRunning;
            adjustableClock.Stop();
        }

        private void endUserInput()
        {
            handlingUserInput = false;
            if (trackWasPlaying)
                adjustableClock.Start();
        }

        protected override ScrollbarContainer CreateScrollbar(Direction direction) => new TimelineScrollbar(this, direction);

        private class TimelineScrollbar : ScrollbarContainer
        {
            private readonly Timeline timeline;

            public TimelineScrollbar(Timeline timeline, Direction scrollDir)
                : base(scrollDir)
            {
                this.timeline = timeline;
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                if (base.OnMouseDown(state, args))
                {
                    timeline.beginUserInput();
                    return true;
                }

                return false;
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                timeline.endUserInput();
                return base.OnMouseUp(state, args);
            }
        }
    }
}
