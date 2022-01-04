// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Linq;
using System.Text;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Groups utility methods used to handle beatmap metadata.
    /// </summary>
    public static class MetadataUtils
    {
        /// <summary>
        /// Returns <see langword="true"/> if the character <paramref name="c"/> can be used in <see cref="BeatmapMetadata.Artist"/> and <see cref="BeatmapMetadata.Title"/> fields.
        /// Characters not matched by this method can be placed in <see cref="BeatmapMetadata.ArtistUnicode"/> and <see cref="BeatmapMetadata.TitleUnicode"/>.
        /// </summary>
        public static bool IsRomanised(char c) => c <= 0xFF;

        /// <summary>
        /// Returns <see langword="true"/> if the string <paramref name="str"/> can be used in <see cref="BeatmapMetadata.Artist"/> and <see cref="BeatmapMetadata.Title"/> fields.
        /// Strings not matched by this method can be placed in <see cref="BeatmapMetadata.ArtistUnicode"/> and <see cref="BeatmapMetadata.TitleUnicode"/>.
        /// </summary>
        public static bool IsRomanised(string? str) => string.IsNullOrEmpty(str) || str.All(IsRomanised);

        /// <summary>
        /// Returns a copy of <paramref name="str"/> with all characters that do not match <see cref="IsRomanised(char)"/> removed.
        /// </summary>
        public static string StripNonRomanisedCharacters(string? str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            var stringBuilder = new StringBuilder(str.Length);

            foreach (char c in str)
            {
                if (IsRomanised(c))
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString().Trim();
        }
    }
}
