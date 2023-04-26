// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public partial class RingPiece : CircularContainer
    {
        private readonly float relativeBorderThickness;

        public RingPiece(float relativeBorderThickness)
        {
            this.relativeBorderThickness = relativeBorderThickness;
            RelativeSizeAxes = Axes.Both;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Masking = true;
            BorderColour = Color4.White;

            Child = new Box
            {
                AlwaysPresent = true,
                Alpha = 0,
                RelativeSizeAxes = Axes.Both
            };
        }

        protected override void Update()
        {
            base.Update();
            BorderThickness = relativeBorderThickness * DrawSize.Y;
        }
    }
}
