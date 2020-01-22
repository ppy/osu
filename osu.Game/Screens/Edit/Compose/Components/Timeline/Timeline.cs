// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    [Cached(typeof(IDistanceSnapProvider))]
    public class Timeline : ZoomableScrollContainer, IDistanceSnapProvider
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
        private void load(IBindable<WorkingBeatmap> beatmap, IAdjustableClock adjustableClock, OsuColour colours)
        {
            this.adjustableClock = adjustableClock;

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
            }, true);
        }

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

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            endUserDrag();
            return base.OnMouseUp(e);
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
        private BindableBeatDivisor beatDivisor { get; set; }

        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        public (Vector2 position, double time) GetSnappedPosition(Vector2 position, double time) => (position, (position.X / Content.DrawWidth) * track.Length);

        public float GetBeatSnapDistanceAt(double referenceTime)
        {
            DifficultyControlPoint difficultyPoint = beatmap.ControlPointInfo.DifficultyPointAt(referenceTime);
            return (float)(100 * beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier / beatDivisor.Value);
        }

        public float DurationToDistance(double referenceTime, double duration)
        {
            double beatLength = beatmap.ControlPointInfo.TimingPointAt(referenceTime).BeatLength / beatDivisor.Value;
            return (float)(duration / beatLength * GetBeatSnapDistanceAt(referenceTime));
        }

        public double DistanceToDuration(double referenceTime, float distance)
        {
            double beatLength = beatmap.ControlPointInfo.TimingPointAt(referenceTime).BeatLength / beatDivisor.Value;
            return distance / GetBeatSnapDistanceAt(referenceTime) * beatLength;
        }

        public double GetSnappedDurationFromDistance(double referenceTime, float distance)
            => beatSnap(referenceTime, DistanceToDuration(referenceTime, distance));

        public float GetSnappedDistanceFromDistance(double referenceTime, float distance)
            => DurationToDistance(referenceTime, beatSnap(referenceTime, DistanceToDuration(referenceTime, distance)));

        /// <summary>
        /// Snaps a duration to the closest beat of a timing point applicable at the reference time.
        /// </summary>
        /// <param name="referenceTime">The time of the timing point which <paramref name="duration"/> resides in.</param>
        /// <param name="duration">The duration to snap.</param>
        /// <returns>A value that represents <paramref name="duration"/> snapped to the closest beat of the timing point.</returns>
        private double beatSnap(double referenceTime, double duration)
        {
            double beatLength = beatmap.ControlPointInfo.TimingPointAt(referenceTime).BeatLength / beatDivisor.Value;

            // A 1ms offset prevents rounding errors due to minute variations in duration
            return (int)((duration + 1) / beatLength) * beatLength;
        }
    }
}
