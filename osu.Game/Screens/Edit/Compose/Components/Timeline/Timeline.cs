// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    [Cached(typeof(IDistanceSnapProvider))]
    [Cached]
    public class Timeline : ZoomableScrollContainer, IDistanceSnapProvider
    {
        public readonly Bindable<bool> WaveformVisible = new Bindable<bool>();
        public readonly IBindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        [Resolved]
        private IAdjustableClock adjustableClock { get; set; }

        public Timeline()
        {
            ZoomDuration = 200;
            ZoomEasing = Easing.OutQuint;
            ScrollbarVisible = false;
        }

        private WaveformGraph waveform;

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap, OsuColour colours)
        {
            Add(waveform = new WaveformGraph
            {
                RelativeSizeAxes = Axes.Both,
                Colour = colours.Blue.Opacity(0.2f),
                LowColour = colours.BlueLighter,
                MidColour = colours.BlueDark,
                HighColour = colours.BlueDarker,
                Depth = float.MaxValue
            });

            // We don't want the centre marker to scroll
            AddInternal(new CentreMarker());

            WaveformVisible.ValueChanged += visible => waveform.FadeTo(visible.NewValue ? 1 : 0, 200, Easing.OutQuint);

            Beatmap.BindTo(beatmap);
            Beatmap.BindValueChanged(b =>
            {
                waveform.Waveform = b.NewValue.Waveform;
                track = b.NewValue.Track;

                MaxZoom = getZoomLevelForVisibleMilliseconds(500);
                MinZoom = getZoomLevelForVisibleMilliseconds(10000);
                Zoom = getZoomLevelForVisibleMilliseconds(2000);
            }, true);
        }

        private float getZoomLevelForVisibleMilliseconds(double milliseconds) => (float)(track.Length / milliseconds);

        /// <summary>
        /// The timeline's scroll position in the last frame.
        /// </summary>
        private float lastScrollPosition;

        /// <summary>
        /// The track time in the last frame.
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

        private Track track;

        protected override void Update()
        {
            base.Update();

            // The extrema of track time should be positioned at the centre of the container when scrolled to the start or end
            Content.Margin = new MarginPadding { Horizontal = DrawWidth / 2 };

            // This needs to happen after transforms are updated, but before the scroll position is updated in base.UpdateAfterChildren
            if (adjustableClock.IsRunning)
                scrollToTrackTime();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (handlingDragInput)
                seekTrackToCurrent();
            else if (!adjustableClock.IsRunning)
            {
                // The track isn't running. There are two cases we have to be wary of:
                // 1) The user flick-drags on this timeline: We want the track to follow us
                // 2) The user changes the track time through some other means (scrolling in the editor or overview timeline): We want to follow the track time

                // The simplest way to cover both cases is by checking whether the scroll position has changed and the audio hasn't been changed externally
                if (Current != lastScrollPosition && adjustableClock.CurrentTime == lastTrackTime)
                    seekTrackToCurrent();
                else
                    scrollToTrackTime();
            }

            lastScrollPosition = Current;
            lastTrackTime = adjustableClock.CurrentTime;
        }

        private void seekTrackToCurrent()
        {
            if (!track.IsLoaded)
                return;

            adjustableClock.Seek(Current / Content.DrawWidth * track.Length);
        }

        private void scrollToTrackTime()
        {
            if (!track.IsLoaded)
                return;

            ScrollTo((float)(adjustableClock.CurrentTime / track.Length) * Content.DrawWidth, false);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (base.OnMouseDown(e))
            {
                beginUserDrag();
                return true;
            }

            return false;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            endUserDrag();
            base.OnMouseUp(e);
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

        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        [Resolved]
        private IBeatSnapProvider beatSnapProvider { get; set; }

        public double GetTimeFromScreenSpacePosition(Vector2 position)
            => getTimeFromPosition(Content.ToLocalSpace(position));

        public (Vector2 position, double time) GetSnappedPosition(Vector2 position, double time) =>
            (position, beatSnapProvider.SnapTime(getTimeFromPosition(position)));

        private double getTimeFromPosition(Vector2 localPosition) =>
            (localPosition.X / Content.DrawWidth) * track.Length;

        public float GetBeatSnapDistanceAt(double referenceTime) => throw new NotImplementedException();

        public float DurationToDistance(double referenceTime, double duration) => throw new NotImplementedException();

        public double DistanceToDuration(double referenceTime, float distance) => throw new NotImplementedException();

        public double GetSnappedDurationFromDistance(double referenceTime, float distance) => throw new NotImplementedException();

        public float GetSnappedDistanceFromDistance(double referenceTime, float distance) => throw new NotImplementedException();
    }
}
