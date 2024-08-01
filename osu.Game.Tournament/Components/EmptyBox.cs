// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class EmptyBox : CompositeDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        private readonly Box box;

        public EmptyBox(int cornerRadius = 0)
        {
            Width = 250;
            Height = 250;

            Masking = true;
            CornerRadius = cornerRadius;
            // CornerExponent = 5;

            InternalChildren = new Drawable[]
            {
                box = new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }
    }
}
