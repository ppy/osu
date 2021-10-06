// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Localisation;

namespace osu.Game.Beatmaps
{
    public static class BeatmapInfoExtensions
    {
        /// <summary>
        /// A user-presentable display title representing this beatmap.
        /// </summary>
        public static string GetDisplayTitle(this IBeatmapInfo beatmapInfo) => $"{getClosestMetadata(beatmapInfo)} {getVersionString(beatmapInfo)}".Trim();

        /// <summary>
        /// A user-presentable display title representing this beatmap, with localisation handling for potentially romanisable fields.
        /// </summary>
        public static RomanisableString GetDisplayTitleRomanisable(this IBeatmapInfo beatmapInfo)
        {
            var metadata = getClosestMetadata(beatmapInfo).GetDisplayTitleRomanisable();
            var versionString = getVersionString(beatmapInfo);

            return new RomanisableString($"{metadata.GetPreferred(true)} {versionString}".Trim(), $"{metadata.GetPreferred(false)} {versionString}".Trim());
        }

        public static string[] GetSearchableTerms(this IBeatmapInfo beatmapInfo) => new[]
        {
            beatmapInfo.DifficultyName
        }.Concat(getClosestMetadata(beatmapInfo).GetSearchableTerms()).Where(s => !string.IsNullOrEmpty(s)).ToArray();

        private static string getVersionString(IBeatmapInfo beatmapInfo) => string.IsNullOrEmpty(beatmapInfo.DifficultyName) ? string.Empty : $"[{beatmapInfo.DifficultyName}]";

        // temporary helper methods until we figure which metadata should be where.
        private static IBeatmapMetadataInfo getClosestMetadata(IBeatmapInfo beatmapInfo) =>
            beatmapInfo.Metadata ?? beatmapInfo.BeatmapSet?.Metadata ?? new BeatmapMetadata();
    }
}
