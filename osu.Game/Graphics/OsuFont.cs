// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics
{
    public struct OsuFont
    {
        public const float DEFAULT_FONT_SIZE = 16;

        public static FontUsage Default => GetFont();

        public static FontUsage GetFont(FontUsage usage, Typeface? typeface = null, float? size = null, FontWeight? weight = null, bool? italics = null, bool? fixedWidth = null)
        {
            string familyString = typeface != null ? getFamilyString(typeface.Value) : usage.Family;
            string weightString = weight != null ? getWeightString(familyString, weight.Value) : usage.Weight;

            return new FontUsage(usage, familyString, size, weightString, italics, fixedWidth);
        }

        public static FontUsage GetFont(Typeface typeface = Typeface.Exo, float size = DEFAULT_FONT_SIZE, FontWeight weight = FontWeight.Regular, bool italics = false, bool fixedWidth = false)
            => new FontUsage(getFamilyString(typeface), size, getWeightString(typeface, weight), italics, fixedWidth);

        private static string getFamilyString(Typeface typeface)
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

        private static string getWeightString(Typeface typeface, FontWeight weight)
            => getWeightString(getFamilyString(typeface), weight);

        private static string getWeightString(string family, FontWeight weight)
        {
            string weightString = weight.ToString();

            // Only exo has an explicit "regular" weight, other fonts do not
            if (family != "Exo2.0" && weight == FontWeight.Regular)
                weightString = string.Empty;

            return weightString;
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
