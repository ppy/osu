// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class ValidationStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Validation";

        /// <summary>
        /// "{0} is missing number or letter"
        /// </summary>
        public static LocalisableString Mixture(string attribute) => new TranslatableString(getKey(@"mixture"), @"{0} is missing number or letter", attribute);

        /// <summary>
        /// "{0} is required"
        /// </summary>
        public static LocalisableString Required(string attribute) => new TranslatableString(getKey(@"required"), @"{0} is required", attribute);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}