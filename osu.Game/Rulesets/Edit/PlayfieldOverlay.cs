// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Edit
{
    public class PlayfieldOverlay : CompositeDrawable
    {
        private readonly Drawable dragBox;

        public PlayfieldOverlay()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new[]
            {
                dragBox = new Container
                {
                    Masking = true,
                    BorderColour = Color4.White,
                    BorderThickness = 2,
                    MaskingSmoothness = 1,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true
                    }
                }
            };
        }

        private Vector2 dragStartPos;

        protected override bool OnDragStart(InputState state)
        {
            dragStartPos = ToLocalSpace(state.Mouse.NativeState.Position);
            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            var dragPos = ToLocalSpace(state.Mouse.NativeState.Position);
            var dragRectangle = RectangleF.FromLTRB(
                Math.Min(dragStartPos.X, dragPos.X),
                Math.Min(dragStartPos.Y, dragPos.Y),
                Math.Max(dragStartPos.X, dragPos.X),
                Math.Max(dragStartPos.Y, dragPos.Y));

            dragBox.Position = dragRectangle.Location;
            dragBox.Size = dragRectangle.Size;

            return true;
        }

        protected override bool OnDragEnd(InputState state)
        {
            return true;
        }
    }
}
