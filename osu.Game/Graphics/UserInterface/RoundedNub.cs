// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public partial class RoundedNub : Nub
    {
        public const float HEIGHT = 15;

        public const float EXPANDED_SIZE = 50;

        public RoundedNub()
        {
            Size = new Vector2(EXPANDED_SIZE, HEIGHT);
        }

        protected override Container CreateNubContainer() =>
            new CircularContainer
            {
                BorderColour = Colour4.White,
                BorderThickness = BORDER_WIDTH,
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
            };
    }
}
