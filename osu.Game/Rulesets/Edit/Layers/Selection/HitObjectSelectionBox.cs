// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    /// <summary>
    /// A box that represents a drag selection.
    /// </summary>
    public class HitObjectSelectionBox : CompositeDrawable
    {
        private readonly Container borderMask;
        private readonly Drawable background;
        private readonly HandleContainer handles;

        private Color4 captureFinishedColour;
        private RectangleF dragRectangle;

        /// <summary>
        /// Creates a new <see cref="HitObjectSelectionBox"/>.
        /// </summary>
        public HitObjectSelectionBox()
        {
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
                    UpdateDragRectangle = SetDragRectangle,
                    FinishDrag = () => FinishCapture()
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            captureFinishedColour = colours.Yellow;
        }

        public void SetDragRectangle(RectangleF rectangle)
        {
            dragRectangle = rectangle;

            var topLeft = Parent.ToLocalSpace(rectangle.TopLeft);
            var bottomRight = Parent.ToLocalSpace(rectangle.BottomRight);

            Position = topLeft;
            Size = bottomRight - topLeft;
        }

        private readonly List<DrawableHitObject> capturedHitObjects = new List<DrawableHitObject>();

        public bool HasCaptured => capturedHitObjects.Count > 0;

        public void AddCaptured(DrawableHitObject hitObject) => capturedHitObjects.Add(hitObject);

        public void ClearCaptured() => capturedHitObjects.Clear();

        /// <summary>
        /// Encloses hitobjects captured through <see cref="BeginCapture"/> in the drag selection box.
        /// </summary>
        public void FinishCapture(bool instant = false)
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

            this.MoveTo(topLeft, instant ? 0 : 100, Easing.OutQuint)
                .ResizeTo(bottomRight - topLeft, instant ? 0 : 100, Easing.OutQuint);

            dragRectangle = RectangleF.FromLTRB(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);

            borderMask.BorderThickness = 3;
            borderMask.FadeColour(captureFinishedColour, 200);

            // Transform into markers to let the user modify the drag selection further.
            background.Delay(50).FadeOut(200);
            handles.FadeIn(200);
        }

        private bool isActive = true;
        public override bool HandleKeyboardInput => isActive;
        public override bool HandleMouseInput => isActive;

        public override void Hide()
        {
            isActive = false;
            this.FadeOut(400, Easing.OutQuint);
        }
    }
}
