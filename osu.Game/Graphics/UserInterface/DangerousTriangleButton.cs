// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class DangerousTriangleButton : TriangleButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.PinkDark;
            Triangles.AccentColours = new Tuple<Color4, Color4>[] {
                Tuple.Create(colours.PinkDarker, colours.Pink)
            };
            // Triangles.ColourDark = colours.PinkDarker;
            // Triangles.ColourLight = colours.Pink;
        }
    }
}
