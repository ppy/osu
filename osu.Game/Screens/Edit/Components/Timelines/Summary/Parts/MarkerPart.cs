// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Graphics;
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
            Add(marker = new MarkerVisualisation());
        }

        protected override bool OnDragStart(DragStartEvent e) => true;

        protected override void OnDrag(DragEvent e)
        {
            seekToPosition(e.ScreenSpaceMousePosition);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            seekToPosition(e.ScreenSpaceMousePosition);
            return true;
        }

        private ScheduledDelegate? scheduledSeek;

        /// <summary>
        /// Seeks the <see cref="SummaryTimeline"/> to the time closest to a position on the screen relative to the <see cref="SummaryTimeline"/>.
        /// </summary>
        /// <param name="screenPosition">The position in screen coordinates.</param>
        private void seekToPosition(Vector2 screenPosition)
        {
            scheduledSeek?.Cancel();
            scheduledSeek = Schedule(() =>
            {
                float markerPos = Math.Clamp(ToLocalSpace(screenPosition).X, 0, DrawWidth);
                editorClock.SeekSmoothlyTo(markerPos / DrawWidth * editorClock.TrackLength);
            });
        }

        protected override void Update()
        {
            base.Update();
            marker.X = (float)editorClock.CurrentTime;
        }

        protected override void LoadBeatmap(EditorBeatmap beatmap)
        {
            // block base call so we don't clear our marker (can be reused on beatmap change).
        }

        private partial class MarkerVisualisation : CompositeDrawable
        {
            public MarkerVisualisation()
            {
                const float box_height = 4;

                Anchor = Anchor.CentreLeft;
                Origin = Anchor.Centre;
                RelativePositionAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;
                AutoSizeAxes = Axes.X;
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(14, box_height),
                    },
                    new Triangle
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.BottomCentre,
                        Scale = new Vector2(1, -1),
                        Size = new Vector2(10, 5),
                        Y = box_height,
                    },
                    new Triangle
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Size = new Vector2(10, 5),
                        Y = -box_height,
                    },
                    new Box
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Size = new Vector2(14, box_height),
                    },
                    new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Y,
                        Width = 1.4f,
                        EdgeSmoothness = new Vector2(1, 0)
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours) => Colour = colours.Red1;
        }
    }
}
