// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    /// <summary>
    /// A box that represents a drag selection.
    /// </summary>
    public class SelectionBox : VisibilityContainer
    {
        /// <summary>
        /// Creates a new <see cref="SelectionBox"/>.
        /// </summary>
        public SelectionBox()
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(-1),
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        BorderColour = Color4.White,
                        BorderThickness = 2,
                        MaskingSmoothness = 1,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.1f,
                            AlwaysPresent = true
                        },
                    }
                }
            };
        }

        public void SetDragRectangle(RectangleF rectangle)
        {
            var topLeft = Parent.ToLocalSpace(rectangle.TopLeft);
            var bottomRight = Parent.ToLocalSpace(rectangle.BottomRight);

            Position = topLeft;
            Size = bottomRight - topLeft;
        }

        public override bool DisposeOnDeathRemoval => true;

        protected override void PopIn() => this.FadeIn(250, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(250, Easing.OutQuint);
    }
}
