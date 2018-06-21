// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Input;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Screens.Compose.Timeline
{
    public class Timeline : ZoomableScrollContainer
    {
        public readonly Bindable<bool> WaveformVisible = new Bindable<bool>();
        public readonly IBindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        private IAdjustableClock adjustableClock;

        public Timeline()
        {
            ZoomDuration = 200;
            ZoomEasing = Easing.OutQuint;
            Zoom = 10;
            ScrollbarVisible = false;
        }

        private WaveformGraph waveform;

        [BackgroundDependencyLoader]
        private void load(IBindableBeatmap beatmap, IAdjustableClock adjustableClock, OsuColour colours)
        {
            this.adjustableClock = adjustableClock;

            Child = waveform = new WaveformGraph
            {
                RelativeSizeAxes = Axes.Both,
                Colour = colours.Blue.Opacity(0.2f),
                LowColour = colours.BlueLighter,
                MidColour = colours.BlueDark,
                HighColour = colours.BlueDarker,
                Depth = float.MaxValue
            };

            // We don't want the centre marker to scroll
            AddInternal(new CentreMarker());

            WaveformVisible.ValueChanged += visible => waveform.FadeTo(visible ? 1 : 0, 200, Easing.OutQuint);

            Beatmap.BindTo(beatmap);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Beatmap.BindValueChanged(b => waveform.Waveform = b.Waveform);
            waveform.Waveform = Beatmap.Value.Waveform;
        }

        /// <summary>
        /// The track's time in the previous frame.
        /// </summary>
        private double lastTrackTime;

        /// <summary>
        /// Whether the user is currently dragging the timeline.
        /// </summary>
        private bool handlingDragInput;

        /// <summary>
        /// Whether the track was playing before a user drag event.
        /// </summary>
        private bool trackWasPlaying;

        protected override void Update()
        {
            base.Update();

            // The extrema of track time should be positioned at the centre of the container when scrolled to the start or end
            Content.Margin = new MarginPadding { Horizontal = DrawWidth / 2 };

            if (handlingDragInput)
            {
                // The user is dragging - the track should always follow the timeline
                seekTrackToCurrent();
            }
            else if (adjustableClock.IsRunning)
            {
                // If the user hasn't provided mouse input but the track is running, always follow the track
                scrollToTrackTime();
            }
            else
            {
                // The track isn't playing, so we want to smooth-scroll once more, and re-enable wheel scrolling
                // There are two cases we have to be wary of:
                // 1) The user scrolls on this timeline: We want the track to follow us
                // 2) The user changes the track time through some other means (scrolling in the editor or overview timeline): We want to follow the track time

                // The simplest way to cover both cases is by checking that inter-frame track times are identical
                if (adjustableClock.CurrentTime == lastTrackTime)
                {
                    // The track hasn't been seeked externally
                    seekTrackToCurrent();
                }
                else
                {
                    // The track has been seeked externally
                    scrollToTrackTime();
                }
            }

            lastTrackTime = adjustableClock.CurrentTime;

            void seekTrackToCurrent()
            {
                if (!(Beatmap.Value.Track is TrackVirtual))
                    adjustableClock.Seek(Current / Content.DrawWidth * Beatmap.Value.Track.Length);
            }

            void scrollToTrackTime()
            {
                if (!(Beatmap.Value.Track is TrackVirtual))
                    ScrollTo((float)(adjustableClock.CurrentTime / Beatmap.Value.Track.Length) * Content.DrawWidth, false);
            }
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            if (base.OnMouseDown(state, args))
            {
                beginUserDrag();
                return true;
            }

            return false;
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            endUserDrag();
            return base.OnMouseUp(state, args);
        }

        private void beginUserDrag()
        {
            handlingDragInput = true;
            trackWasPlaying = adjustableClock.IsRunning;
            adjustableClock.Stop();
        }

        private void endUserDrag()
        {
            handlingDragInput = false;
            if (trackWasPlaying)
                adjustableClock.Start();
        }
    }
}
