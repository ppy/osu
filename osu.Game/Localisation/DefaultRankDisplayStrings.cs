// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class DefaultRankDisplayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.DefaultRankDisplay";

        /// <summary>
        /// "Play samples on rank change"
        /// </summary>
        public static LocalisableString PlaySamplesOnRankChange => new TranslatableString(getKey(@"play_samples_on_rank_change"), @"Play samples on rank change");

        /// <summary>
        /// "Hide during gameplay"
        /// </summary>
        public static LocalisableString HideDuringGameplay => new TranslatableString(getKey(@"hide_during_gameplay"), @"Hide during gameplay");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
