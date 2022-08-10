// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class EditorSetupRulesetStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.EditorSetupRuleset";

        /// <summary>
        /// "Ruleset ({0})"
        /// </summary>
        public static LocalisableString Ruleset(string arg0) => new TranslatableString(getKey(@"ruleset"), @"Ruleset ({0})", arg0);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
