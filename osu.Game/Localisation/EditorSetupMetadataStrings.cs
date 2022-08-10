// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class EditorSetupMetadataStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.EditorSetupMetadata";

        /// <summary>
        /// "Metadata"
        /// </summary>
        public static LocalisableString Metadata => new TranslatableString(getKey(@"metadata"), @"Metadata");

        /// <summary>
        /// "Romanised Artist"
        /// </summary>
        public static LocalisableString RomanisedArtist => new TranslatableString(getKey(@"romanised_artist"), @"Romanised Artist");

        /// <summary>
        /// "Romanised Title"
        /// </summary>
        public static LocalisableString RomanisedTitle => new TranslatableString(getKey(@"romanised_title"), @"Romanised Title");

        /// <summary>
        /// "Creator"
        /// </summary>
        public static LocalisableString Creator => new TranslatableString(getKey(@"creator"), @"Creator");

        /// <summary>
        /// "Difficulty Name"
        /// </summary>
        public static LocalisableString DifficultyName => new TranslatableString(getKey(@"difficulty_name"), @"Difficulty Name");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
