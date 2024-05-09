// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class DeleteConfirmationDialogStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.DeleteConfirmationDialog";

        /// <summary>
        /// "Caution"
        /// </summary>
        public static LocalisableString HeaderText => new TranslatableString(getKey(@"header_text"), @"Caution");

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
