// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public static class ColourHitErrorMeterStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HUD.ColourHitError";

        /// <summary>
        /// "Judgement count"
        /// </summary>
        public static LocalisableString JudgementCount => new TranslatableString(getKey(@"judgement_count"), "Judgement count");

        /// <summary>
        /// "The number of displayed judgements"
        /// </summary>
        public static LocalisableString JudgementCountDescription => new TranslatableString(getKey(@"judgement_count_description"), "The number of displayed judgements");

        /// <summary>
        /// "Judgement spacing"
        /// </summary>
        public static LocalisableString JudgementSpacing => new TranslatableString(getKey(@"judgement_spacing"), "Judgement spacing");

        /// <summary>
        /// "The space between each displayed judgement"
        /// </summary>
        public static LocalisableString JudgementSpacingDescription => new TranslatableString(getKey(@"judgement_spacing_description"), "The space between each displayed judgement");

        /// <summary>
        /// "Judgement shape"
        /// </summary>
        public static LocalisableString JudgementShape => new TranslatableString(getKey(@"judgement_shape"), "Judgement shape");

        /// <summary>
        /// "The shape of each displayed judgement"
        /// </summary>
        public static LocalisableString JudgementShapeDescription => new TranslatableString(getKey(@"judgement_shape_description"), "The shape of each displayed judgement");

        /// <summary>
        /// "Circle"
        /// </summary>
        public static LocalisableString ShapeStyleCircle => new TranslatableString(getKey(@"shape_style_cricle"), "Circle");

        /// <summary>
        /// "Square"
        /// </summary>
        public static LocalisableString ShapeStyleSquare => new TranslatableString(getKey(@"shape_style_square"), "Square");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
