// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    /// <summary>
    /// The part of the timeline that displays the current position of the song.
    /// </summary>
    internal class MarkerPart : TimelinePart
    {
        private readonly Drawable marker;

        public MarkerPart()
        {
            Add(marker = new MarkerVisualisation());
        }

        protected override bool OnDragStart(InputState state) => true;
        protected override bool OnDragEnd(InputState state) => true;
        protected override bool OnDrag(InputState state)
        {
            seekToPosition(state.Mouse.NativeState.Position);
            return true;
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            seekToPosition(state.Mouse.NativeState.Position);
            return true;
        }

        /// <summary>
        /// Seeks the <see cref="SummaryTimeline"/> to the time closest to a position on the screen relative to the <see cref="SummaryTimeline"/>.
        /// </summary>
        /// <param name="screenPosition">The position in screen coordinates.</param>
        private void seekToPosition(Vector2 screenPosition)
        {
            if (Beatmap.Value == null)
                return;

            float markerPos = MathHelper.Clamp(ToLocalSpace(screenPosition).X, 0, DrawWidth);
            seekTo(markerPos / DrawWidth * Beatmap.Value.Track.Length);
        }

        private void seekTo(double time) => Beatmap.Value?.Track.Seek(time);

        protected override void Update()
        {
            base.Update();
            marker.X = (float)(Beatmap.Value?.Track.CurrentTime ?? 0);
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
