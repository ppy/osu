// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class StorageErrorDialogStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.StorageErrorDialog";

        /// <summary>
        /// "osu! storage error"
        /// </summary>
        public static LocalisableString StorageError => new TranslatableString(getKey(@"storage_error"), @"osu! storage error");

        /// <summary>
        /// "The specified osu! data location (&quot;{0}&quot;) is not accessible. If it is on external storage, please reconnect the device and try again."
        /// </summary>
        public static LocalisableString LocationIsNotAccessible(string? loc) => new TranslatableString(getKey(@"location_is_not_accessible"), @"The specified osu! data location (""{0}"") is not accessible. If it is on external storage, please reconnect the device and try again.", loc);

        /// <summary>
        /// "The specified osu! data location (&quot;{0}&quot;) is empty. If you have moved the files, please close osu! and move them back."
        /// </summary>
        public static LocalisableString LocationIsEmpty(string? loc2) => new TranslatableString(getKey(@"location_is_empty"), @"The specified osu! data location (""{0}"") is empty. If you have moved the files, please close osu! and move them back.", loc2);

        /// <summary>
        /// "Try again"
        /// </summary>
        public static LocalisableString TryAgain => new TranslatableString(getKey(@"try_again"), @"Try again");

        /// <summary>
        /// "Use default location until restart"
        /// </summary>
        public static LocalisableString UseDefaultLocation => new TranslatableString(getKey(@"use_default_location"), @"Use default location until restart");

        /// <summary>
        /// "Reset to default location"
        /// </summary>
        public static LocalisableString ResetToDefaultLocation => new TranslatableString(getKey(@"reset_to_default_location"), @"Reset to default location");

        /// <summary>
        /// "Start fresh at specified location"
        /// </summary>
        public static LocalisableString StartFresh => new TranslatableString(getKey(@"start_fresh"), @"Start fresh at specified location");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
