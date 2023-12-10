// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class AimErrorMeterStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HUD.PositionMeterStrings";

        /// <summary>
        /// "Judgement position size"
        /// </summary>
        public static LocalisableString JudgementSize => new TranslatableString(getKey(@"judgement_line_thickness"), "Judgement position size");

        /// <summary>
        /// "How big of judgement position should be."
        /// </summary>
        public static LocalisableString JudgementSizeDescription => new TranslatableString(getKey("judgement_line_thickness"), "How big of judgement position should be.");

        /// <summary>
        /// "Judgement position style"
        /// </summary>
        public static LocalisableString JudgementStyle => new TranslatableString(getKey(@"judgement_line_thickness"), "Judgement position style");

        /// <summary>
        /// "The style of judgement position."
        /// </summary>
        public static LocalisableString JudgementStyleDescription => new TranslatableString(getKey("judgement_line_thickness"), "The style of judgement position.");

        /// <summary>
        /// "Average position size"
        /// </summary>
        public static LocalisableString AverageSize => new TranslatableString(getKey(@"judgement_line_thickness"), "Average position size");

        /// <summary>
        /// "How big of average position should be."
        /// </summary>
        public static LocalisableString AverageSizeDescription => new TranslatableString(getKey("judgement_line_thickness"), "How big of average position should be.");

        /// <summary>
        /// "Average position style"
        /// </summary>
        public static LocalisableString AverageStyle => new TranslatableString(getKey(@"judgement_line_thickness"), "Average position style");

        /// <summary>
        /// "The style of average position."
        /// </summary>
        public static LocalisableString AverageStyleDescription => new TranslatableString(getKey("judgement_line_thickness"), "The style of average position.");

        /// <summary>
        /// "X"
        /// </summary>
        public static LocalisableString StyleX => new TranslatableString(getKey("style_x"), "X");

        /// <summary>
        /// "+"
        /// </summary>
        public static LocalisableString StylePlus => new TranslatableString(getKey("style_plus"), "+");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
