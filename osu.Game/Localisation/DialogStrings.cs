// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class DialogStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Dialog";

        /// <summary>
        /// "Caution"
        /// </summary>
        public static LocalisableString CautionHeaderText => new TranslatableString(getKey(@"header_text"), @"Caution");

        /// <summary>
        /// "Are you sure you want to delete the following:"
        /// </summary>
        public static LocalisableString DeletionHeaderText => new TranslatableString(getKey(@"deletion_header_text"), @"Are you sure you want to delete the following:");

        /// <summary>
        /// "Yes. Go for it."
        /// </summary>
        public static LocalisableString Confirm => new TranslatableString(getKey(@"confirm"), @"Yes. Go for it.");

        /// <summary>
        /// "No! Abort mission"
        /// </summary>
        public static LocalisableString Cancel => new TranslatableString(getKey(@"cancel"), @"No! Abort mission");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
