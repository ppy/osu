// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace osu.Game.Users.Drawables
{
    public partial class StatusIcon : CircularContainer
    {
        public StatusIcon()
        {
            Size = new Vector2(25);
            BorderThickness = 4;
            BorderColour = Colour4.White; // the colour is being applied through Colour - since it's multiplicative it applies to the border as well
            Masking = true;
            Child = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.White.Opacity(0)
            };
        }
    }
}
