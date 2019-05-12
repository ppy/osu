// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    /// <summary>
    /// The part of the timeline that displays the current position of the song.
    /// </summary>
    public class MarkerPart : TimelinePart
    {
        private readonly Drawable marker;

        private readonly IAdjustableClock adjustableClock;

        public MarkerPart(IAdjustableClock adjustableClock)
        {
            this.adjustableClock = adjustableClock;

            Add(marker = new MarkerVisualisation());
        }

        protected override bool OnDragStart(DragStartEvent e) => true;
        protected override bool OnDragEnd(DragEndEvent e) => true;

        protected override bool OnDrag(DragEvent e)
        {
            seekToPosition(e.ScreenSpaceMousePosition);
            return true;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            seekToPosition(e.ScreenSpaceMousePosition);
            return true;
        }

        private ScheduledDelegate scheduledSeek;

        /// <summary>
        /// Seeks the <see cref="SummaryTimeline"/> to the time closest to a position on the screen relative to the <see cref="SummaryTimeline"/>.
        /// </summary>
        /// <param name="screenPosition">The position in screen coordinates.</param>
        private void seekToPosition(Vector2 screenPosition)
        {
            scheduledSeek?.Cancel();
            scheduledSeek = Schedule(() =>
            {
                if (Beatmap.Value == null)
                    return;

                float markerPos = MathHelper.Clamp(ToLocalSpace(screenPosition).X, 0, DrawWidth);
                adjustableClock.Seek(markerPos / DrawWidth * Beatmap.Value.Track.Length);
            });
        }

        protected override void Update()
        {
            base.Update();
            marker.X = (float)adjustableClock.CurrentTime;
        }

        protected override void LoadBeatmap(WorkingBeatmap beatmap)
        {
            // block base call so we don't clear our marker (can be reused on beatmap change).
        }

        private class MarkerVisualisation : CompositeDrawable
        {
            public MarkerVisualisation()
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.Centre;
                RelativePositionAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;
                AutoSizeAxes = Axes.X;
                InternalChildren = new Drawable[]
                {
                    new Triangle
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.BottomCentre,
                        Scale = new Vector2(1, -1),
                        Size = new Vector2(10, 5),
                    },
                    new Triangle
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Size = new Vector2(10, 5)
                    },
                    new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Y,
                        Width = 2,
                        EdgeSmoothness = new Vector2(1, 0)
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours) => Colour = colours.Red;
        }
    }
}
