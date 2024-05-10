// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    internal static class ArgonSnapColouring
    {
        public static Color4 SnapColourFor(int divisor, OsuColour? colours)
        {
            if (colours is null) return Color4.Gray;

            switch (divisor)
            {
                case 1:
                    return colours.Gray8;
                case 2:
                    return colours.Red;
                case 4:
                    return colours.BlueDarker;
                case 8:
                    return colours.YellowDark;
                case 16:
                    return colours.PurpleDark;
                case 3:
                    return colours.PinkDark;
                case 6:
                    return colours.Purple;
                case 12:
                    return colours.PinkDarker;
                default:
                    return colours.Gray7;
            }
        }
    }
}
