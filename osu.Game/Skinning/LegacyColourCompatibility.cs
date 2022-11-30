// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Compatibility methods to apply osu!stable quirks to colours. Should be used for legacy skins only.
    /// </summary>
    public static class LegacyColourCompatibility
    {
        /// <summary>
        /// Forces an alpha of 1 if a given <see cref="Color4"/> is fully transparent.
        /// </summary>
        /// <remarks>
        /// This is equivalent to setting colour post-constructor in osu!stable.
        /// </remarks>
        /// <param name="colour">The <see cref="Color4"/> to disallow zero alpha on.</param>
        /// <returns>The resultant <see cref="Color4"/>.</returns>
        public static Color4 DisallowZeroAlpha(Color4 colour)
        {
            if (colour.A == 0)
                colour.A = 1;
            return colour;
        }

        /// <summary>
        /// Applies a <see cref="Color4"/> to a <see cref="Drawable"/>, doubling the alpha value into the <see cref="Drawable.Alpha"/> property.
        /// </summary>
        /// <remarks>
        /// This is equivalent to setting colour in the constructor in osu!stable.
        /// </remarks>
        /// <param name="drawable">The <see cref="Drawable"/> to apply the colour to.</param>
        /// <param name="colour">The <see cref="Color4"/> to apply.</param>
        /// <returns>The given <paramref name="drawable"/>.</returns>
        public static T ApplyWithDoubledAlpha<T>(T drawable, Color4 colour)
            where T : Drawable
        {
            drawable.Alpha = colour.A;
            drawable.Colour = DisallowZeroAlpha(colour);
            return drawable;
        }
    }
}
