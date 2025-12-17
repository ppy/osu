// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osuTK.Graphics;

namespace osu.Game.Seasonal
{
    /// <summary>
    /// General configuration setting for seasonal event adjustments to the game.
    /// </summary>
    public static class SeasonalUIConfig
    {
        public static readonly bool ENABLED = true;

        public static readonly Color4 PRIMARY_COLOUR_1 = Color4Extensions.FromHex(@"D32F2F");

        public static readonly Color4 PRIMARY_COLOUR_2 = Color4Extensions.FromHex(@"388E3C");

        public static readonly Color4 AMBIENT_COLOUR_1 = Color4Extensions.FromHex(@"FFFFCC");

        public static readonly Color4 AMBIENT_COLOUR_2 = Color4Extensions.FromHex(@"FFE4B5");
    }
}
