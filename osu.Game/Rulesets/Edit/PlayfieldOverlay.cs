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
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using System.Linq;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Edit
{
    public class PlayfieldOverlay : CompositeDrawable
    {
        private readonly static Color4 selection_normal_colour = Color4.White;
        private readonly static Color4 selection_attached_colour = OsuColour.FromHex("eeaa00");

        private readonly Container dragBox;

        private readonly Playfield playfield;

        public PlayfieldOverlay(Playfield playfield)
        {
            this.playfield = playfield;

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
                        Alpha = 0.1f
                    }
                }
            };
        }

        private Vector2 dragStartPos;
        private RectangleF dragRectangle;
        private List<DrawableHitObject> capturedHitObjects = new List<DrawableHitObject>();

        protected override bool OnDragStart(InputState state)
        {
            dragStartPos = ToLocalSpace(state.Mouse.NativeState.Position);
            dragBox.Position = dragStartPos;
            dragBox.Size = Vector2.Zero;
            dragBox.FadeTo(1);
            dragBox.FadeColour(selection_normal_colour);
            dragBox.BorderThickness = 2;
            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            var dragPos = ToLocalSpace(state.Mouse.NativeState.Position);
            dragRectangle = RectangleF.FromLTRB(
                Math.Min(dragStartPos.X, dragPos.X),
                Math.Min(dragStartPos.Y, dragPos.Y),
                Math.Max(dragStartPos.X, dragPos.X),
                Math.Max(dragStartPos.Y, dragPos.Y));

            dragBox.Position = dragRectangle.Location;
            dragBox.Size = dragRectangle.Size;

            updateCapturedHitObjects();

            return true;
        }

        private void updateCapturedHitObjects()
        {
            capturedHitObjects.Clear();

            foreach (var obj in playfield.HitObjects.Objects)
            {
                if (!obj.IsAlive || !obj.IsPresent)
                    continue;

                var objectPosition = obj.Parent.ToScreenSpace(obj.SelectionPoint);
                if (dragRectangle.Contains(ToLocalSpace(objectPosition)))
                    capturedHitObjects.Add(obj);
            }
        }

        protected override bool OnDragEnd(InputState state)
        {
            if (capturedHitObjects.Count == 0)
                dragBox.FadeOut(400, Easing.OutQuint);
            else
            {
                // Move the rectangle to cover the hitobjects
                var topLeft = new Vector2(float.MaxValue, float.MaxValue);
                var bottomRight = new Vector2(float.MinValue, float.MinValue);

                foreach (var obj in capturedHitObjects)
                {
                    topLeft = Vector2.ComponentMin(topLeft, ToLocalSpace(obj.SelectionQuad.TopLeft));
                    bottomRight = Vector2.ComponentMax(bottomRight, ToLocalSpace(obj.SelectionQuad.BottomRight));
                }

                topLeft -= new Vector2(5);
                bottomRight += new Vector2(5);

                dragBox.MoveTo(topLeft, 200, Easing.OutQuint)
                       .ResizeTo(bottomRight - topLeft, 200, Easing.OutQuint)
                       .FadeColour(selection_attached_colour, 200, Easing.OutQuint);
                dragBox.BorderThickness = 3;
            }

            return true;
        }
    }
}
