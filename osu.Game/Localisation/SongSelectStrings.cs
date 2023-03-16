// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class SongSelectStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.SongSelect";

        /// <summary>
        /// "Local"
        /// </summary>
        public static LocalisableString LocallyModified => new TranslatableString(getKey(@"locally_modified"), @"Local");

        /// <summary>
        /// "Has been locally modified"
        /// </summary>
        public static LocalisableString LocallyModifiedTooltip => new TranslatableString(getKey(@"locally_modified_tooltip"), @"Has been locally modified");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
