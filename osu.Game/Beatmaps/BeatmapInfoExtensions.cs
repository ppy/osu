// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;

namespace osu.Game.Beatmaps
{
    public static class BeatmapInfoExtensions
    {
        /// <summary>
        /// A user-presentable display title representing this beatmap.
        /// </summary>
        public static string GetDisplayTitle(this IBeatmapInfo beatmapInfo) => $"{beatmapInfo.Metadata.GetDisplayTitle()} {getVersionString(beatmapInfo)}".Trim();

        /// <summary>
        /// A user-presentable display title representing this beatmap, with localisation handling for potentially romanisable fields.
        /// </summary>
        public static RomanisableString GetDisplayTitleRomanisable(this IBeatmapInfo beatmapInfo, bool includeDifficultyName = true, bool includeCreator = true)
        {
            var metadata = beatmapInfo.Metadata.GetDisplayTitleRomanisable(includeCreator);

            if (includeDifficultyName)
            {
                string versionString = getVersionString(beatmapInfo);
                return new RomanisableString($"{metadata.GetPreferred(true)} {versionString}".Trim(), $"{metadata.GetPreferred(false)} {versionString}".Trim());
            }

            return new RomanisableString($"{metadata.GetPreferred(true)}".Trim(), $"{metadata.GetPreferred(false)}".Trim());
        }

        public static ReadOnlySpan<string> GetSearchableTerms(this IBeatmapInfo beatmapInfo)
        {
            Span<string> terms = new string[8];
            int i = 0;
            if (!string.IsNullOrEmpty(beatmapInfo.DifficultyName))
                terms[i++] = beatmapInfo.DifficultyName;
            var metadata = beatmapInfo.Metadata;
            if (!string.IsNullOrEmpty(metadata.Author.Username))
                terms[i++] = metadata.Author.Username;
            if (!string.IsNullOrEmpty(metadata.Artist))
                terms[i++] = metadata.Artist;
            if (!string.IsNullOrEmpty(metadata.ArtistUnicode))
                terms[i++] = metadata.ArtistUnicode;
            if (!string.IsNullOrEmpty(metadata.Title))
                terms[i++] = metadata.Title;
            if (!string.IsNullOrEmpty(metadata.TitleUnicode))
                terms[i++] = metadata.TitleUnicode;
            if (!string.IsNullOrEmpty(metadata.Source))
                terms[i++] = metadata.Source;
            if (!string.IsNullOrEmpty(metadata.Tags))
                terms[i++] = metadata.Tags;
            return terms[..i];
        }

        private static string getVersionString(IBeatmapInfo beatmapInfo) => string.IsNullOrEmpty(beatmapInfo.DifficultyName) ? string.Empty : $"[{beatmapInfo.DifficultyName}]";
    }
}
