// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using osu.Framework.Localisation;

namespace osu.Game.Beatmaps
{
    public static class BeatmapMetadataInfoExtensions
    {
        /// <summary>
        /// An array of all searchable terms provided in contained metadata.
        /// </summary>
        public static string[] GetSearchableTerms(this IBeatmapMetadataInfo metadataInfo)
        {
            var termsList = new List<string>(MAX_SEARCHABLE_TERM_COUNT);
            CollectSearchableTerms(metadataInfo, termsList);
            return termsList.ToArray();
        }

        internal const int MAX_SEARCHABLE_TERM_COUNT = 7;

        internal static void CollectSearchableTerms(IBeatmapMetadataInfo metadataInfo, IList<string> termsList)
        {
            addIfNotNull(metadataInfo.Author.Username);
            addIfNotNull(metadataInfo.Artist);
            addIfNotNull(metadataInfo.ArtistUnicode);
            addIfNotNull(metadataInfo.Title);
            addIfNotNull(metadataInfo.TitleUnicode);
            addIfNotNull(metadataInfo.Source);
            addIfNotNull(metadataInfo.Tags);

            void addIfNotNull(string s)
            {
                if (!string.IsNullOrEmpty(s))
                    termsList.Add(s);
            }
        }

        /// <summary>
        /// A user-presentable display title representing this metadata.
        /// </summary>
        public static string GetDisplayTitle(this IBeatmapMetadataInfo metadataInfo)
        {
            string author = string.IsNullOrEmpty(metadataInfo.Author.Username) ? string.Empty : $" ({metadataInfo.Author.Username})";

            string artist = string.IsNullOrEmpty(metadataInfo.Artist) ? "unknown artist" : metadataInfo.Artist;
            string title = string.IsNullOrEmpty(metadataInfo.Title) ? "unknown title" : metadataInfo.Title;

            return $"{artist} - {title}{author}".Trim();
        }

        /// <summary>
        /// A user-presentable display title representing this beatmap, with localisation handling for potentially romanisable fields.
        /// </summary>
        public static RomanisableString GetDisplayTitleRomanisable(this IBeatmapMetadataInfo metadataInfo, bool includeCreator = true)
        {
            string author = !includeCreator || string.IsNullOrEmpty(metadataInfo.Author.Username) ? string.Empty : $"({metadataInfo.Author.Username})";
            string artistUnicode = string.IsNullOrEmpty(metadataInfo.ArtistUnicode) ? metadataInfo.Artist : metadataInfo.ArtistUnicode;
            string titleUnicode = string.IsNullOrEmpty(metadataInfo.TitleUnicode) ? metadataInfo.Title : metadataInfo.TitleUnicode;

            return new RomanisableString($"{artistUnicode} - {titleUnicode} {author}".Trim(), $"{metadataInfo.Artist} - {metadataInfo.Title} {author}".Trim());
        }
    }
}
