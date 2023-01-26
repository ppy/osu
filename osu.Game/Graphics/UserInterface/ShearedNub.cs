// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public partial class ShearedNub : Nub
    {
        public const int HEIGHT = 30;
        public const float EXPANDED_SIZE = 50;

        public static readonly Vector2 SHEAR = new Vector2(0.15f, 0);

        public ShearedNub()
        {
            Size = new Vector2(EXPANDED_SIZE, HEIGHT);
        }

        protected override Container CreateNubContainer() =>
            new Container
            {
                Shear = SHEAR,
                BorderColour = Colour4.White,
                BorderThickness = BORDER_WIDTH,
                Masking = true,
                CornerRadius = 5,
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
            };
    }
}
