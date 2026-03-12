// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public class RankedPlayColourScheme
    {
        public required Color4 Primary;
        public required Color4 PrimaryDarker;
        public required Color4 PrimaryDarkest;
        public required Color4 Surface;
        public required Color4 SurfaceBorder;

        public static RankedPlayColourScheme Blue => new RankedPlayColourScheme
        {
            Primary = Color4Extensions.FromHex("5EBFFF"),
            PrimaryDarker = Color4Extensions.FromHex("4382FF"),
            PrimaryDarkest = Color4Extensions.FromHex("5C55FF"),
            Surface = Color4Extensions.FromHex("33303D"),
            SurfaceBorder = Color4Extensions.FromHex("514c5e"),
        };

        public static RankedPlayColourScheme Red => new RankedPlayColourScheme
        {
            Primary = Color4Extensions.FromHex("FF8198"),
            PrimaryDarker = Color4Extensions.FromHex("F94D92"),
            PrimaryDarkest = Color4Extensions.FromHex("B6104D"),
            Surface = Color4Extensions.FromHex("242023"),
            SurfaceBorder = Color4Extensions.FromHex("403b3f"),
        };
    }
}
