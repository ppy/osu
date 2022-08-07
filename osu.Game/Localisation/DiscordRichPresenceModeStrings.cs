// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class DiscordRichPresenceModeStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.DiscordRichPresenceMode";

        /// <summary>
        /// "Hide identifiable information"
        /// </summary>
        public static LocalisableString HideIdentifiableInformation => new TranslatableString(getKey(@"hide_identifiable_information"), @"Hide identifiable information");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}