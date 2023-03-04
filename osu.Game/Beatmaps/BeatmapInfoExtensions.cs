// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
            string[] terms = new string[8];
            int i = 0;
            var metadata = beatmapInfo.Metadata;
            addIfNotNull(beatmapInfo.DifficultyName);
            addIfNotNull(metadata.Author.Username);
            addIfNotNull(metadata.Artist);
            addIfNotNull(metadata.ArtistUnicode);
            addIfNotNull(metadata.Title);
            addIfNotNull(metadata.TitleUnicode);
            addIfNotNull(metadata.Source);
            addIfNotNull(metadata.Tags);
            return terms.AsSpan(0, i);

            void addIfNotNull(string? s)
            {
                if (!string.IsNullOrEmpty(s))
                    terms[i++] = s;
            }
        }

        private static string getVersionString(IBeatmapInfo beatmapInfo) => string.IsNullOrEmpty(beatmapInfo.DifficultyName) ? string.Empty : $"[{beatmapInfo.DifficultyName}]";
    }
}
