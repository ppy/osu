// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces
{
    public class ElongatedCirclePiece : CirclePiece
    {
        public ElongatedCirclePiece()
        {
            RelativeSizeAxes = Axes.Y;
        }

        protected override void Update()
        {
            base.Update();

            var padding = Content.DrawHeight * Content.Width / 2;

            Content.Padding = new MarginPadding
            {
                Left = padding,
                Right = padding,
            };

            Width = Parent.DrawSize.X + DrawHeight;
        }
    }
}
