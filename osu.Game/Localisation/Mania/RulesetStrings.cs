// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mania
{
    public static class RulesetStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mania.Ruleset";

        /// <summary>
        /// "Keys"
        /// </summary>
        public static LocalisableString VariantDescription => new TranslatableString(getKey(@"variant_description"), @"Keys");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
