// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class RoomStatusPillStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.RoomStatusPill";

        /// <summary>
        /// "Ended"
        /// </summary>
        public static LocalisableString Ended => new TranslatableString(getKey(@"ended"), @"Ended");

        /// <summary>
        /// "Playing"
        /// </summary>
        public static LocalisableString Playing => new TranslatableString(getKey(@"playing"), @"Playing");

        /// <summary>
        /// "Open (Private)"
        /// </summary>
        public static LocalisableString OpenPrivate => new TranslatableString(getKey(@"open_private"), @"Open (Private)");

        /// <summary>
        /// "Open"
        /// </summary>
        public static LocalisableString Open => new TranslatableString(getKey(@"open"), @"Open");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}