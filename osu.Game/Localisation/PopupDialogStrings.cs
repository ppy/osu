// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class PopupDialogStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.PopupDialog";

        /// <summary>
        /// "Are you sure you want to update this beatmap?"
        /// </summary>
        public static LocalisableString UpdateLocallyModifiedText => new TranslatableString(getKey(@"update_locally_modified_text"), @"Are you sure you want to update this beatmap?");

        /// <summary>
        /// "This will discard all local changes you have on that beatmap."
        /// </summary>
        public static LocalisableString UpdateLocallyModifiedDescription => new TranslatableString(getKey(@"update_locally_modified_description"), @"This will discard all local changes you have on that beatmap.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
