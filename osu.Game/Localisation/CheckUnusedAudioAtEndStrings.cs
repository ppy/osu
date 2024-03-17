// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class CheckUnusedAudioAtEndStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.CheckUnusedAudioAtEnd";

        /// <summary>
        /// "{0}% of the audio is not mapped."
        /// </summary>
        public static LocalisableString OfTheAudioIsNot(double percentageLeft) => new TranslatableString(getKey(@"of_the_audio_is_not"), @"{0}% of the audio is not mapped.", percentageLeft);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
