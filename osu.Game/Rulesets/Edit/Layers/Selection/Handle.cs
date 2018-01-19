// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    /// <summary>
    /// Represents a marker visible on the border of a <see cref="HandleContainer"/> which exposes
    /// properties that are used to resize a <see cref="HitObjectSelectionBox"/>.
    /// </summary>
    public class Handle : CompositeDrawable
    {
        private const float marker_size = 10;

        /// <summary>
        /// Invoked when this <see cref="Handle"/> requires the current drag rectangle.
        /// </summary>
        public Func<RectangleF> GetDragRectangle;

        /// <summary>
        /// Invoked when this <see cref="Handle"/> wants to update the drag rectangle.
        /// </summary>
        public Action<RectangleF> UpdateDragRectangle;

        /// <summary>
        /// Invoked when this <see cref="Handle"/> has finished updates to the drag rectangle.
        /// </summary>
        public Action FinishDrag;

        private Color4 normalColour;
        private Color4 hoverColour;

        public Handle()
        {
            Size = new Vector2(marker_size);

            InternalChild = new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Child = new Box { RelativeSizeAxes = Axes.Both }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = normalColour = colours.Yellow;
            hoverColour = colours.YellowDarker;
        }

        protected override bool OnDragStart(InputState state) => true;

        protected override bool OnDrag(InputState state)
        {
            var currentRectangle = GetDragRectangle();

            float left = currentRectangle.Left;
            float right = currentRectangle.Right;
            float top = currentRectangle.Top;
            float bottom = currentRectangle.Bottom;

            // Apply modifications to the capture rectangle
            if ((Anchor & Anchor.y0) > 0)
                top += state.Mouse.Delta.Y;
            else if ((Anchor & Anchor.y2) > 0)
                bottom += state.Mouse.Delta.Y;

            if ((Anchor & Anchor.x0) > 0)
                left += state.Mouse.Delta.X;
            else if ((Anchor & Anchor.x2) > 0)
                right += state.Mouse.Delta.X;

            UpdateDragRectangle(RectangleF.FromLTRB(left, top, right, bottom));
            return true;
        }

        protected override bool OnDragEnd(InputState state)
        {
            FinishDrag();
            return true;
        }

        protected override bool OnHover(InputState state)
        {
            this.FadeColour(hoverColour, 100);
            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            this.FadeColour(normalColour, 100);
        }
    }
}
