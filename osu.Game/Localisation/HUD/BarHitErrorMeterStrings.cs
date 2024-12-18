// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public static class BarHitErrorMeterStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HUD.BarHitErrorMeter";

        /// <summary>
        /// "Judgement line thickness"
        /// </summary>
        public static LocalisableString JudgementLineThickness => new TranslatableString(getKey(@"judgement_line_thickness"), "Judgement line thickness");

        /// <summary>
        /// "How thick the individual lines should be."
        /// </summary>
        public static LocalisableString JudgementLineThicknessDescription => new TranslatableString(getKey(@"judgement_line_thickness_description"), "How thick the individual lines should be.");

        /// <summary>
        /// "Show colour bars"
        /// </summary>
        public static LocalisableString ColourBarVisibility => new TranslatableString(getKey(@"colour_bar_visibility"), "Show colour bars");

        /// <summary>
        /// "Show moving average arrow"
        /// </summary>
        public static LocalisableString ShowMovingAverage => new TranslatableString(getKey(@"show_moving_average"), "Show moving average arrow");

        /// <summary>
        /// "Whether an arrow should move beneath the bar showing the average error."
        /// </summary>
        public static LocalisableString ShowMovingAverageDescription => new TranslatableString(getKey(@"show_moving_average_description"), "Whether an arrow should move beneath the bar showing the average error.");

        /// <summary>
        /// "Centre marker style"
        /// </summary>
        public static LocalisableString CentreMarkerStyle => new TranslatableString(getKey(@"centre_marker_style"), "Centre marker style");

        /// <summary>
        /// "How to signify the centre of the display"
        /// </summary>
        public static LocalisableString CentreMarkerStyleDescription => new TranslatableString(getKey(@"centre_marker_style_description"), "How to signify the centre of the display");

        /// <summary>
        /// "None"
        /// </summary>
        public static LocalisableString CentreMarkerStylesNone => new TranslatableString(getKey(@"centre_marker_styles_none"), "None");

        /// <summary>
        /// "Circle"
        /// </summary>
        public static LocalisableString CentreMarkerStylesCircle => new TranslatableString(getKey(@"centre_marker_styles_circle"), "Circle");

        /// <summary>
        /// "Line"
        /// </summary>
        public static LocalisableString CentreMarkerStylesLine => new TranslatableString(getKey(@"centre_marker_styles_line"), "Line");

        /// <summary>
        /// "Label style"
        /// </summary>
        public static LocalisableString LabelStyle => new TranslatableString(getKey(@"label_style"), "Label style");

        /// <summary>
        /// "How to show early/late extremities"
        /// </summary>
        public static LocalisableString LabelStyleDescription => new TranslatableString(getKey(@"label_style_description"), "How to show early/late extremities");

        /// <summary>
        /// "None"
        /// </summary>
        public static LocalisableString LabelStylesNone => new TranslatableString(getKey(@"label_styles_none"), "None");

        /// <summary>
        /// "Icons"
        /// </summary>
        public static LocalisableString LabelStylesIcons => new TranslatableString(getKey(@"label_styles_icons"), "Icons");

        /// <summary>
        /// "Text"
        /// </summary>
        public static LocalisableString LabelStylesText => new TranslatableString(getKey(@"label_styles_text"), "Text");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
