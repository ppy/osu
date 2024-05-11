// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public partial class ArgonElongatedCirclePiece : ArgonCirclePiece
    {
        public ArgonElongatedCirclePiece()
        {
            RelativeSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AccentColour = ColourInfo.GradientVertical(
                new Color4(241, 161, 0, 255),
                new Color4(167, 111, 0, 255)
            );
        }

        protected override void Update()
        {
            base.Update();
            Width = Parent!.DrawSize.X + DrawHeight;
        }
    }
}
