// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class PenSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.PenSettings";

        /// <summary>
        /// "Tablet (External)"
        /// </summary>
        public static LocalisableString TabletExternal => new TranslatableString(getKey(@"tablet_external"), @"Tablet (External)");

        /// <summary>
        /// "Cursor sensitivity"
        /// </summary>
        public static LocalisableString CursorSensitivity => new TranslatableString(getKey(@"cursor_sensitivity"), @"Cursor sensitivity");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
