// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Localisation;

namespace osu.Game.Beatmaps
{
    public static class BeatmapMetadataInfoExtensions
    {
        /// <summary>
        /// An array of all searchable terms provided in contained metadata.
        /// </summary>
        public static string[] GetSearchableTerms(this IBeatmapMetadataInfo metadataInfo) => new[]
        {
            metadataInfo.Author.Username,
            metadataInfo.Artist,
            metadataInfo.ArtistUnicode,
            metadataInfo.Title,
            metadataInfo.TitleUnicode,
            metadataInfo.Source,
            metadataInfo.Tags
        }.Where(s => !string.IsNullOrEmpty(s)).ToArray();

        /// <summary>
        /// A user-presentable display title representing this metadata.
        /// </summary>
        public static string GetDisplayTitle(this IBeatmapMetadataInfo metadataInfo)
        {
            string author = string.IsNullOrEmpty(metadataInfo.Author.Username) ? string.Empty : $"({metadataInfo.Author})";
            return $"{metadataInfo.Artist} - {metadataInfo.Title} {author}".Trim();
        }

        /// <summary>
        /// A user-presentable display title representing this beatmap, with localisation handling for potentially romanisable fields.
        /// </summary>
        public static RomanisableString GetDisplayTitleRomanisable(this IBeatmapMetadataInfo metadataInfo, bool includeCreator = true)
        {
            string author = !includeCreator || string.IsNullOrEmpty(metadataInfo.Author.Username) ? string.Empty : $"({metadataInfo.Author})";
            string artistUnicode = string.IsNullOrEmpty(metadataInfo.ArtistUnicode) ? metadataInfo.Artist : metadataInfo.ArtistUnicode;
            string titleUnicode = string.IsNullOrEmpty(metadataInfo.TitleUnicode) ? metadataInfo.Title : metadataInfo.TitleUnicode;

            return new RomanisableString($"{artistUnicode} - {titleUnicode} {author}".Trim(), $"{metadataInfo.Artist} - {metadataInfo.Title} {author}".Trim());
        }
    }
}
