// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osuTK;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    /// <summary>
    /// The part of the timeline that displays the current position of the song.
    /// </summary>
    public partial class MarkerPart : TimelinePart
    {
        private Drawable marker = null!;

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(marker = new CentreMarker
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.Centre,
                RelativePositionAxes = Axes.X,
                Width = 10,
                TriangleHeightRatio = 0.5f
            });
        }

        private double? lastSeekTime;

        protected override bool OnDragStart(DragStartEvent e) => true;

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);
            seekToPosition(e.ScreenSpaceMousePosition, instant: false);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            base.OnDragEnd(e);
            seekToPosition(e.ScreenSpaceMousePosition, instant: true);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            seekToPosition(e.ScreenSpaceMousePosition, instant: true);
            return true;
        }

        /// <summary>
        /// Seeks the <see cref="SummaryTimeline"/> to the time closest to a position on the screen relative to the <see cref="SummaryTimeline"/>.
        /// </summary>
        /// <param name="screenPosition">The position in screen coordinates.</param>
        /// <param name="instant">Whether the seek should be instant (drag end, mouse button press) or debounced (drag in progress).</param>
        private void seekToPosition(Vector2 screenPosition, bool instant)
        {
            float markerPos = Math.Clamp(ToLocalSpace(screenPosition).X, 0, DrawWidth);
            double seekDestination = markerPos / DrawWidth * editorClock.TrackLength;
            marker.X = (float)seekDestination;

            if (editorClock.IsRunning && !instant && lastSeekTime != null && Time.Current - lastSeekTime < NowPlayingOverlay.TRACK_DRAG_SEEK_DEBOUNCE)
                return;

            editorClock.SeekSmoothlyTo(seekDestination);

            lastSeekTime = instant ? null : Time.Current;
        }

        protected override void Update()
        {
            base.Update();

            if (!IsDragged)
                marker.X = (float)editorClock.CurrentTime;
        }

        protected override void LoadBeatmap(EditorBeatmap beatmap)
        {
            // block base call so we don't clear our marker (can be reused on beatmap change).
        }
    }
}
