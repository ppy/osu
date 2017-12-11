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
    public class SelectionLayer : CompositeDrawable
    {
        private readonly Playfield playfield;

        public SelectionLayer(Playfield playfield)
        {
            this.playfield = playfield;

            RelativeSizeAxes = Axes.Both;
        }

        private DragBox dragBox;

        protected override bool OnDragStart(InputState state)
        {
            dragBox?.Hide();
            AddInternal(dragBox = new DragBox(ToLocalSpace(state.Mouse.NativeState.Position)));
            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            dragBox.ExpandTo(ToLocalSpace(state.Mouse.NativeState.Position));

            updateCapturedHitObjects();
            return true;
        }

        private List<DrawableHitObject> capturedHitObjects = new List<DrawableHitObject>();
        private void updateCapturedHitObjects()
        {
            capturedHitObjects.Clear();

            foreach (var obj in playfield.HitObjects.Objects)
            {
                if (!obj.IsAlive || !obj.IsPresent)
                    continue;

                var objectPosition = obj.ToScreenSpace(obj.SelectionPoint);
                if (dragBox.ScreenSpaceDrawQuad.Contains(objectPosition))
                    capturedHitObjects.Add(obj);
            }
        }

        protected override bool OnDragEnd(InputState state)
        {
            if (capturedHitObjects.Count == 0)
                dragBox.Hide();
            else
                dragBox.Capture(capturedHitObjects);
            return true;
        }

        protected override bool OnClick(InputState state)
        {
            dragBox?.Hide();
            return true;
        }
    }

    public class DragBox : CompositeDrawable
    {
        private readonly Drawable background;
        private readonly Vector2 startPos;

        public DragBox(Vector2 startPos)
        {
            this.startPos = startPos;

            Masking = true;
            BorderColour = Color4.White;
            BorderThickness = 2;
            MaskingSmoothness = 1;
            InternalChild = background = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0.1f,
                AlwaysPresent = true
            };
        }

        public void ExpandTo(Vector2 position)
        {
            var trackingRectangle = RectangleF.FromLTRB(
                Math.Min(startPos.X, position.X),
                Math.Min(startPos.Y, position.Y),
                Math.Max(startPos.X, position.X),
                Math.Max(startPos.Y, position.Y));

            Position = trackingRectangle.Location;
            Size = trackingRectangle.Size;
        }

        public void Capture(IEnumerable<DrawableHitObject> hitObjects)
        {
            // Move the rectangle to cover the hitobjects
            var topLeft = new Vector2(float.MaxValue, float.MaxValue);
            var bottomRight = new Vector2(float.MinValue, float.MinValue);

            foreach (var obj in hitObjects)
            {
                topLeft = Vector2.ComponentMin(topLeft, Parent.ToLocalSpace(obj.SelectionQuad.TopLeft));
                bottomRight = Vector2.ComponentMax(bottomRight, Parent.ToLocalSpace(obj.SelectionQuad.BottomRight));
            }

            topLeft -= new Vector2(5);
            bottomRight += new Vector2(5);

            this.MoveTo(topLeft, 200, Easing.OutQuint)
                .ResizeTo(bottomRight - topLeft, 200, Easing.OutQuint);

            background.FadeOut(200);

            BorderThickness = 3;
        }

        private bool isActive = true;
        public override bool HandleInput => isActive;

        public override void Hide()
        {
            isActive = false;
            this.FadeOut(400, Easing.OutQuint).Expire();
        }
    }
}
