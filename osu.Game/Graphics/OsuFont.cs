// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics
{
    public struct OsuFont
    {
        public const float DEFAULT_FONT_SIZE = 16;

        public static FontUsage Default => GetFont();

        public static FontUsage GetFont(Typeface typeface = Typeface.Exo, float size = DEFAULT_FONT_SIZE, FontWeight weight = FontWeight.Medium, bool italics = false, bool fixedWidth = false)
            => new FontUsage(GetFamilyString(typeface), size, GetWeightString(typeface, weight), italics, fixedWidth);

        public static string GetFamilyString(Typeface typeface)
        {
            switch (typeface)
            {
                case Typeface.Exo:
                    return "Exo2.0";
                case Typeface.FontAwesome:
                    return "FontAwesome";
                case Typeface.Venera:
                    return "Venera";
            }

            return null;
        }

        public static string GetWeightString(Typeface typeface, FontWeight weight)
            => GetWeightString(GetFamilyString(typeface), weight);

        public static string GetWeightString(string family, FontWeight weight)
        {
            string weightString = weight.ToString();

            // Only exo has an explicit "regular" weight, other fonts do not
            if (family != "Exo2.0" && weight == FontWeight.Regular)
                weightString = string.Empty;

            return weightString;
        }
    }

    public static class OsuFontExtensions
    {
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
        FontAwesome,
        Venera,
    }

    public enum FontWeight
    {
        Light,
        Regular,
        Medium,
        SemiBold,
        Bold,
        Black
    }
}
