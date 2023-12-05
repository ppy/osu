// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Screens.Select;

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

        public static bool Match(this IBeatmapInfo beatmapInfo, params FilterCriteria.OptionalTextFilter[] filters)
        {
            foreach (var filter in filters)
            {
                if (filter.Matches(beatmapInfo.DifficultyName))
                    continue;

                if (BeatmapMetadataInfoExtensions.Match(beatmapInfo.Metadata, filter))
                    continue;

                // failed to match a single filter at all - fail the whole match.
                return false;
            }

            // got through all filters without failing any - pass the whole match.
            return true;
        }

        private static string getVersionString(IBeatmapInfo beatmapInfo) => string.IsNullOrEmpty(beatmapInfo.DifficultyName) ? string.Empty : $"[{beatmapInfo.DifficultyName}]";
    }
}
