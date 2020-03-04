// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics
{
    public static class OsuFont
    {
        /// <summary>
        /// The default font size.
        /// </summary>
        public const float DEFAULT_FONT_SIZE = 16;

        /// <summary>
        /// The default font.
        /// </summary>
        public static FontUsage Default => GetFont();

        public static FontUsage Numeric => GetFont(Typeface.Venera, weight: FontWeight.Bold);

        public static FontUsage Torus => GetFont(Typeface.Torus, weight: FontWeight.Regular);

        /// <summary>
        /// Retrieves a <see cref="FontUsage"/>.
        /// </summary>
        /// <param name="typeface">The font typeface.</param>
        /// <param name="size">The size of the text in local space. For a value of 16, a single line will have a height of 16px.</param>
        /// <param name="weight">The font weight.</param>
        /// <param name="italics">Whether the font is italic.</param>
        /// <param name="fixedWidth">Whether all characters should be spaced the same distance apart.</param>
        /// <returns>The <see cref="FontUsage"/>.</returns>
        public static FontUsage GetFont(Typeface typeface = Typeface.Exo, float size = DEFAULT_FONT_SIZE, FontWeight weight = FontWeight.Medium, bool italics = false, bool fixedWidth = false)
            => new FontUsage(GetFamilyString(typeface), size, GetWeightString(typeface, weight), italics, fixedWidth);

        /// <summary>
        /// Retrieves the string representation of a <see cref="Typeface"/>.
        /// </summary>
        /// <param name="typeface">The <see cref="Typeface"/>.</param>
        /// <returns>The string representation.</returns>
        public static string GetFamilyString(Typeface typeface)
        {
            switch (typeface)
            {
                case Typeface.Exo:
                    return "Exo2.0";

                case Typeface.Venera:
                    return "Venera";

                case Typeface.Torus:
                    return "Torus";
            }

            return null;
        }

        /// <summary>
        /// Retrieves the string representation of a <see cref="FontWeight"/>.
        /// </summary>
        /// <param name="typeface">The <see cref="Typeface"/>.</param>
        /// <param name="weight">The <see cref="FontWeight"/>.</param>
        /// <returns>The string representation of <paramref name="weight"/> in the specified <paramref name="typeface"/>.</returns>
        public static string GetWeightString(Typeface typeface, FontWeight weight)
            => GetWeightString(GetFamilyString(typeface), weight);

        /// <summary>
        /// Retrieves the string representation of a <see cref="FontWeight"/>.
        /// </summary>
        /// <param name="family">The family string.</param>
        /// <param name="weight">The <see cref="FontWeight"/>.</param>
        /// <returns>The string representation of <paramref name="weight"/> in the specified <paramref name="family"/>.</returns>
        public static string GetWeightString(string family, FontWeight weight) => weight.ToString();
    }

    public static class OsuFontExtensions
    {
        /// <summary>
        /// Creates a new <see cref="FontUsage"/> by applying adjustments to this <see cref="FontUsage"/>.
        /// </summary>
        /// <param name="usage">The base <see cref="FontUsage"/>.</param>
        /// <param name="typeface">The font typeface. If null, the value is copied from this <see cref="FontUsage"/>.</param>
        /// <param name="size">The text size. If null, the value is copied from this <see cref="FontUsage"/>.</param>
        /// <param name="weight">The font weight. If null, the value is copied from this <see cref="FontUsage"/>.</param>
        /// <param name="italics">Whether the font is italic. If null, the value is copied from this <see cref="FontUsage"/>.</param>
        /// <param name="fixedWidth">Whether all characters should be spaced apart the same distance. If null, the value is copied from this <see cref="FontUsage"/>.</param>
        /// <returns>The resulting <see cref="FontUsage"/>.</returns>
        public static FontUsage With(this FontUsage usage, Typeface? typeface = null, float? size = null, FontWeight? weight = null, bool? italics = null, bool? fixedWidth = null)
        {
            string familyString = typeface != null ? OsuFont.GetFamilyString(typeface.Value) : usage.Family;
            string weightString = weight != null ? OsuFont.GetWeightString(familyString, weight.Value) : usage.Weight;

            return usage.With(familyString, size, weightString, italics, fixedWidth);
        }
    }

    public enum Typeface
    {
        Exo,
        Venera,
        Torus
    }

    public enum FontWeight
    {
        /// <summary>
        /// Equivalent to weight 300.
        /// </summary>
        Light = 300,

        /// <summary>
        /// Equivalent to weight 400.
        /// </summary>
        Regular = 400,

        /// <summary>
        /// Equivalent to weight 500.
        /// </summary>
        Medium = 500,

        /// <summary>
        /// Equivalent to weight 600.
        /// </summary>
        SemiBold = 600,

        /// <summary>
        /// Equivalent to weight 700.
        /// </summary>
        Bold = 700,

        /// <summary>
        /// Equivalent to weight 900.
        /// </summary>
        Black = 900
    }
}
