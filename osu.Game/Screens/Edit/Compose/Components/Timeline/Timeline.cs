// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    [Cached]
    public partial class Timeline : ZoomableScrollContainer
    {
        private const float timeline_height = 80;

        private readonly Drawable userContent;

        private bool alwaysShowControlPoints;

        public bool AlwaysShowControlPoints
        {
            get => alwaysShowControlPoints;
            set
            {
                if (value == alwaysShowControlPoints)
                    return;

                alwaysShowControlPoints = value;
                controlPointsVisible.TriggerChange();
            }
        }

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        /// <summary>
        /// The timeline's scroll position in the last frame.
        /// </summary>
        private double lastScrollPosition;

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

        /// <summary>
        /// The timeline zoom level at a 1x zoom scale.
        /// </summary>
        private float defaultTimelineZoom;

        private WaveformGraph waveform = null!;

        private TimelineTickDisplay ticks = null!;

        private TimelineTimingChangeDisplay controlPoints = null!;

        private Bindable<float> waveformOpacity = null!;
        private Bindable<float> objectsOpacity = null!;

        private Bindable<bool> controlPointsVisible = null!;
        private Bindable<bool> ticksVisible = null!;

        private double trackLengthForZoom;

        public Timeline(Drawable userContent)
        {
            this.userContent = userContent;

            RelativeSizeAxes = Axes.X;
            Height = timeline_height;

            ZoomDuration = 200;
            ZoomEasing = Easing.OutQuint;
            ScrollbarVisible = false;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OverlayColourProvider colourProvider, OsuConfigManager config)
        {
            CentreMarker centreMarker;

            // We don't want the centre marker to scroll
            AddInternal(centreMarker = new CentreMarker
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Width = 8,
                TriangleHeightRatio = 0.8f,
                Colour = colourProvider.Colour2
            });

            AddRange(new Drawable[]
            {
                ticks = new TimelineTickDisplay(),
                new Box
                {
                    Name = "zero marker",
                    RelativeSizeAxes = Axes.Y,
                    Width = TimelineTickDisplay.TICK_WIDTH / 2,
                    Origin = Anchor.TopCentre,
                    Colour = colourProvider.Background1,
                },
                controlPoints = new TimelineTimingChangeDisplay
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = timeline_height,
                    Children = new[]
                    {
                        waveform = new WaveformGraph
                        {
                            RelativeSizeAxes = Axes.Both,
                            BaseColour = colours.Blue.Opacity(0.2f),
                            LowColour = colours.BlueLighter,
                            MidColour = colours.BlueDark,
                            HighColour = colours.BlueDarker,
                        },
                        centreMarker.CreateProxy(),
                        ticks.CreateProxy(),
                        userContent,
                    }
                },
            });

            waveformOpacity = config.GetBindable<float>(OsuSetting.EditorWaveformOpacity);
            objectsOpacity = config.GetBindable<float>(OsuSetting.EditorTimelineObjectsOpacity);

            controlPointsVisible = config.GetBindable<bool>(OsuSetting.EditorTimelineShowTimingChanges);
            ticksVisible = config.GetBindable<bool>(OsuSetting.EditorTimelineShowTicks);

            editorClock.TrackChanged += updateWaveform;
            updateWaveform();

            Zoom = (float)(defaultTimelineZoom * editorBeatmap.TimelineZoom);
        }

        private void updateWaveform()
        {
            waveform.Waveform = beatmap.Value.Waveform;
            Scheduler.AddOnce(applyVisualOffset, beatmap);
        }

        private void applyVisualOffset(IBindable<WorkingBeatmap> beatmap)
        {
            waveform.RelativePositionAxes = Axes.X;

            if (beatmap.Value.Track.Length > 0)
                waveform.X = -(float)(Editor.WAVEFORM_VISUAL_OFFSET / beatmap.Value.Track.Length);
            else
            {
                // sometimes this can be the case immediately after a track switch.
                // reschedule with the hope that the track length eventually populates.
                Scheduler.AddOnce(applyVisualOffset, beatmap);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            waveformOpacity.BindValueChanged(_ => updateWaveformOpacity(), true);
            objectsOpacity.BindValueChanged(_ => updateObjectsOpacity(), true);

            ticksVisible.BindValueChanged(visible => ticks.FadeTo(visible.NewValue ? 1 : 0, 200, Easing.OutQuint), true);

            controlPointsVisible.BindValueChanged(visible =>
            {
                if (visible.NewValue || alwaysShowControlPoints)
                    controlPoints.FadeIn(400, Easing.OutQuint);
                else
                    controlPoints.FadeOut(200, Easing.OutQuint);
            }, true);
        }

        private void updateWaveformOpacity() =>
            waveform.FadeTo(waveformOpacity.Value, 200, Easing.OutQuint);

        private void updateObjectsOpacity() =>
            userContent.FadeTo(objectsOpacity.Value, 200, Easing.OutQuint);

        protected override void Update()
        {
            base.Update();

            // The extrema of track time should be positioned at the centre of the container when scrolled to the start or end
            Content.Margin = new MarginPadding { Horizontal = DrawWidth / 2 };

            // This needs to happen after transforms are updated, but before the scroll position is updated in base.UpdateAfterChildren
            if (editorClock.IsRunning)
                scrollToTrackTime();

            if (editorClock.TrackLength != trackLengthForZoom)
            {
                defaultTimelineZoom = getZoomLevelForVisibleMilliseconds(6000);

                float minimumZoom = getZoomLevelForVisibleMilliseconds(10000);
                float maximumZoom = getZoomLevelForVisibleMilliseconds(500);

                float initialZoom = (float)Math.Clamp(defaultTimelineZoom * (editorBeatmap.TimelineZoom == 0 ? 1 : editorBeatmap.TimelineZoom), minimumZoom, maximumZoom);

                SetupZoom(initialZoom, minimumZoom, maximumZoom);

                float getZoomLevelForVisibleMilliseconds(double milliseconds) => Math.Max(1, (float)(editorClock.TrackLength / milliseconds));

                trackLengthForZoom = editorClock.TrackLength;
            }
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            // if this is not a precision scroll event, let the editor handle the seek itself (for snapping support)
            if (!e.AltPressed && !e.IsPrecise)
                return false;

            return base.OnScroll(e);
        }

        protected override void OnZoomChanged()
        {
            base.OnZoomChanged();
            editorBeatmap.TimelineZoom = Zoom / defaultTimelineZoom;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (handlingDragInput)
                seekTrackToCurrent();
            else if (!editorClock.IsRunning)
            {
                // The track isn't running. There are three cases we have to be wary of:
                // 1) The user flick-drags on this timeline and we are applying an interpolated seek on the clock, until interrupted by 2 or 3.
                // 2) The user changes the track time through some other means (scrolling in the editor or overview timeline; clicking a hitobject etc.). We want the timeline to track the clock's time.
                // 3) An ongoing seek transform is running from an external seek. We want the timeline to track the clock's time.

                // The simplest way to cover the first two cases is by checking whether the scroll position has changed and the audio hasn't been changed externally
                // Checking IsSeeking covers the third case, where the transform may not have been applied yet.
                if (Current != lastScrollPosition && editorClock.CurrentTime == lastTrackTime && !editorClock.IsSeeking)
                    seekTrackToCurrent();
                else
                    scrollToTrackTime();
            }

            lastScrollPosition = Current;
            lastTrackTime = editorClock.CurrentTime;
        }

        private void seekTrackToCurrent()
        {
            double target = TimeAtPosition(Current);
            editorClock.Seek(Math.Min(editorClock.TrackLength, target));
        }

        private void scrollToTrackTime()
        {
            if (editorClock.TrackLength == 0)
                return;

            // covers the case where the user starts playback after a drag is in progress.
            // we want to ensure the clock is always stopped during drags to avoid weird audio playback.
            if (handlingDragInput)
                editorClock.Stop();

            float position = PositionAtTime(editorClock.CurrentTime);
            ScrollTo(position, false);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (base.OnMouseDown(e))
                beginUserDrag();

            // handling right button as well breaks context menus inside the timeline, only handle left button for now.
            return e.Button == MouseButton.Left;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            endUserDrag();
            base.OnMouseUp(e);
        }

        private void beginUserDrag()
        {
            handlingDragInput = true;
            trackWasPlaying = editorClock.IsRunning;
            editorClock.Stop();
        }

        private void endUserDrag()
        {
            handlingDragInput = false;
            if (trackWasPlaying)
                editorClock.Start();
        }

        [Resolved]
        private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

        /// <summary>
        /// The total amount of time visible on the timeline.
        /// </summary>
        public double VisibleRange => editorClock.TrackLength / Zoom;

        public double TimeAtPosition(double x)
        {
            return x / Content.DrawWidth * editorClock.TrackLength;
        }

        public float PositionAtTime(double time)
        {
            return (float)(time / editorClock.TrackLength * Content.DrawWidth);
        }

        public SnapResult FindSnappedPositionAndTime(Vector2 screenSpacePosition)
        {
            double time = TimeAtPosition(Content.ToLocalSpace(screenSpacePosition).X);
            return new SnapResult(screenSpacePosition, beatSnapProvider.SnapTime(time));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (editorClock.IsNotNull())
                editorClock.TrackChanged -= updateWaveform;
        }
    }
}
