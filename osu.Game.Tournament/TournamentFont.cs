// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Tournament
{
    public static class TournamentFont
    {
        /// <summary>
        /// The default font size.
        /// </summary>
        public const float DEFAULT_FONT_SIZE = 16;

        /// <summary>
        /// Retrieves a <see cref="FontUsage"/>.
        /// </summary>
        /// <param name="typeface">The font typeface.</param>
        /// <param name="size">The size of the text in local space. For a value of 16, a single line will have a height of 16px.</param>
        /// <param name="weight">The font weight.</param>
        /// <param name="italics">Whether the font is italic.</param>
        /// <param name="fixedWidth">Whether all characters should be spaced the same distance apart.</param>
        /// <returns>The <see cref="FontUsage"/>.</returns>
        public static FontUsage GetFont(TournamentTypeface typeface = TournamentTypeface.Aquatico, float size = DEFAULT_FONT_SIZE, FontWeight weight = FontWeight.Medium, bool italics = false, bool fixedWidth = false)
            => new FontUsage(GetFamilyString(typeface), size, GetWeightString(typeface, weight), italics, fixedWidth);

        /// <summary>
        /// Retrieves the string representation of a <see cref="TournamentTypeface"/>.
        /// </summary>
        /// <param name="typeface">The <see cref="TournamentTypeface"/>.</param>
        /// <returns>The string representation.</returns>
        public static string GetFamilyString(TournamentTypeface typeface)
        {
            switch (typeface)
            {
                case TournamentTypeface.Aquatico:
                    return "Aquatico";
            }

            return null;
        }

        /// <summary>
        /// Retrieves the string representation of a <see cref="FontWeight"/>.
        /// </summary>
        /// <param name="typeface">The <see cref="TournamentTypeface"/>.</param>
        /// <param name="weight">The <see cref="FontWeight"/>.</param>
        /// <returns>The string representation of <paramref name="weight"/> in the specified <paramref name="typeface"/>.</returns>
        public static string GetWeightString(TournamentTypeface typeface, FontWeight weight)
            => GetWeightString(GetFamilyString(typeface), weight);

        /// <summary>
        /// Retrieves the string representation of a <see cref="FontWeight"/>.
        /// </summary>
        /// <param name="family">The family string.</param>
        /// <param name="weight">The <see cref="FontWeight"/>.</param>
        /// <returns>The string representation of <paramref name="weight"/> in the specified <paramref name="family"/>.</returns>
        public static string GetWeightString(string family, FontWeight weight)
        {
            string weightString = weight.ToString();

            // Only exo has an explicit "regular" weight, other fonts do not
            if (weight == FontWeight.Regular && family != GetFamilyString(TournamentTypeface.Aquatico) && family != GetFamilyString(TournamentTypeface.Aquatico))
                weightString = string.Empty;

            return weightString;
        }
    }

    public enum TournamentTypeface
    {
        Aquatico
    }
}
