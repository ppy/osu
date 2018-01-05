// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    /// <summary>
    /// A box that represents a drag selection.
    /// </summary>
    public class HitObjectSelectionBox : CompositeDrawable
    {
        public readonly Bindable<SelectionInfo> Selection = new Bindable<SelectionInfo>();

        /// <summary>
        /// The <see cref="DrawableHitObject"/>s that can be selected through a drag-selection.
        /// </summary>
        public IEnumerable<DrawableHitObject> CapturableObjects;

        private readonly Container borderMask;
        private readonly Drawable background;
        private readonly HandleContainer handles;

        private Color4 captureFinishedColour;

        private readonly Vector2 startPos;

        /// <summary>
        /// Creates a new <see cref="HitObjectSelectionBox"/>.
        /// </summary>
        /// <param name="startPos">The point at which the drag was initiated, in the parent's coordinates.</param>
        public HitObjectSelectionBox(Vector2 startPos)
        {
            this.startPos = startPos;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(-1),
                    Child = borderMask = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        BorderColour = Color4.White,
                        BorderThickness = 2,
                        MaskingSmoothness = 1,
                        Child = background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.1f,
                            AlwaysPresent = true
                        },
                    }
                },
                handles = new HandleContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    GetDragRectangle = () => dragRectangle,
                    UpdateDragRectangle = updateDragRectangle,
                    FinishDrag = FinishCapture
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            captureFinishedColour = colours.Yellow;
        }

        /// <summary>
        /// The secondary corner of the drag selection box. A rectangle will be fit between the starting position and this value.
        /// </summary>
        public Vector2 DragEndPosition { set => updateDragRectangle(RectangleF.FromLTRB(startPos.X, startPos.Y, value.X, value.Y)); }

        private RectangleF dragRectangle;
        private void updateDragRectangle(RectangleF rectangle)
        {
            dragRectangle = rectangle;

            Position = new Vector2(
                Math.Min(rectangle.Left, rectangle.Right),
                Math.Min(rectangle.Top, rectangle.Bottom));

            Size = new Vector2(
                Math.Max(rectangle.Left, rectangle.Right) - Position.X,
                Math.Max(rectangle.Top, rectangle.Bottom) - Position.Y);
        }

        private readonly List<DrawableHitObject> capturedHitObjects = new List<DrawableHitObject>();

        /// <summary>
        /// Processes hitobjects to determine which ones are captured by the drag selection.
        /// Captured hitobjects will be enclosed by the drag selection upon <see cref="FinishCapture"/>.
        /// </summary>
        public void BeginCapture()
        {
            capturedHitObjects.Clear();

            foreach (var obj in CapturableObjects)
            {
                if (!obj.IsAlive || !obj.IsPresent)
                    continue;

                if (ScreenSpaceDrawQuad.Contains(obj.SelectionPoint))
                    capturedHitObjects.Add(obj);
            }
        }

        /// <summary>
        /// Encloses hitobjects captured through <see cref="BeginCapture"/> in the drag selection box.
        /// </summary>
        public void FinishCapture()
        {
            if (capturedHitObjects.Count == 0)
            {
                Hide();
                return;
            }

            // Move the rectangle to cover the hitobjects
            var topLeft = new Vector2(float.MaxValue, float.MaxValue);
            var bottomRight = new Vector2(float.MinValue, float.MinValue);

            foreach (var obj in capturedHitObjects)
            {
                topLeft = Vector2.ComponentMin(topLeft, Parent.ToLocalSpace(obj.SelectionQuad.TopLeft));
                bottomRight = Vector2.ComponentMax(bottomRight, Parent.ToLocalSpace(obj.SelectionQuad.BottomRight));
            }

            topLeft -= new Vector2(5);
            bottomRight += new Vector2(5);

            this.MoveTo(topLeft, 200, Easing.OutQuint)
                .ResizeTo(bottomRight - topLeft, 200, Easing.OutQuint);

            dragRectangle = RectangleF.FromLTRB(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);

            borderMask.BorderThickness = 3;
            borderMask.FadeColour(captureFinishedColour, 200);

            // Transform into markers to let the user modify the drag selection further.
            background.Delay(50).FadeOut(200);
            handles.FadeIn(200);

            Selection.Value = new SelectionInfo
            {
                Objects = capturedHitObjects,
                SelectionQuad = Parent.ToScreenSpace(dragRectangle)
            };
        }

        private bool isActive = true;
        public override bool HandleInput => isActive;

        public override void Hide()
        {
            isActive = false;
            this.FadeOut(400, Easing.OutQuint).Expire();

            Selection.Value = null;
        }
    }
}
