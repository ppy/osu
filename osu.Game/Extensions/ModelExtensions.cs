// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Users;

namespace osu.Game.Extensions
{
    public static class ModelExtensions
    {
        /// <summary>
        /// Get the relative path in osu! storage for this file.
        /// </summary>
        /// <param name="fileInfo">The file info.</param>
        /// <returns>A relative file path.</returns>
        public static string GetStoragePath(this IFileInfo fileInfo) => Path.Combine(fileInfo.Hash.Remove(1), fileInfo.Hash.Remove(2), fileInfo.Hash);

        /// <summary>
        /// Returns a user-facing string representing the <paramref name="model"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Non-interface types without special handling will fall back to <see cref="object.ToString()"/>.
        /// </para>
        /// <para>
        /// Warning: This method is _purposefully_ not called <c>GetDisplayTitle()</c> like the others, because otherwise
        /// extension method type inference rules cause this method to call itself and cause a stack overflow.
        /// </para>
        /// </remarks>
        public static string GetDisplayString(this object? model)
        {
            string? result = null;

            switch (model)
            {
                case IBeatmapSetInfo beatmapSetInfo:
                    result = beatmapSetInfo.Metadata.GetDisplayTitle();
                    break;

                case IBeatmapInfo beatmapInfo:
                    result = beatmapInfo.GetDisplayTitle();
                    break;

                case IBeatmapMetadataInfo metadataInfo:
                    result = metadataInfo.GetDisplayTitle();
                    break;

                case IScoreInfo scoreInfo:
                    result = scoreInfo.GetDisplayTitle();
                    break;

                case IRulesetInfo rulesetInfo:
                    result = rulesetInfo.Name;
                    break;

                case IUser user:
                    result = user.Username;
                    break;
            }

            // fallback in case none of the above happens to match.
            result ??= model?.ToString() ?? @"null";
            return result;
        }

        /// <summary>
        /// Check whether this <see cref="IRulesetInfo"/>'s online ID is within the range that defines it as a legacy ruleset (ie. either osu!, osu!taiko, osu!catch or osu!mania).
        /// </summary>
        public static bool IsLegacyRuleset(this IRulesetInfo ruleset) => ruleset.OnlineID >= 0 && ruleset.OnlineID <= ILegacyRuleset.MAX_LEGACY_RULESET_ID;

        /// <summary>
        /// Check whether the online ID of two <see cref="IBeatmapSetInfo"/>s match.
        /// </summary>
        /// <param name="instance">The instance to compare.</param>
        /// <param name="other">The other instance to compare against.</param>
        /// <returns>Whether online IDs match. If either instance is missing an online ID, this will return false.</returns>
        public static bool MatchesOnlineID(this IBeatmapSetInfo? instance, IBeatmapSetInfo? other) => matchesOnlineID(instance, other);

        /// <summary>
        /// Check whether the online ID of two <see cref="IBeatmapInfo"/>s match.
        /// </summary>
        /// <param name="instance">The instance to compare.</param>
        /// <param name="other">The other instance to compare against.</param>
        /// <returns>Whether online IDs match. If either instance is missing an online ID, this will return false.</returns>
        public static bool MatchesOnlineID(this IBeatmapInfo? instance, IBeatmapInfo? other) => matchesOnlineID(instance, other);

        /// <summary>
        /// Check whether the online ID of two <see cref="IRulesetInfo"/>s match.
        /// </summary>
        /// <param name="instance">The instance to compare.</param>
        /// <param name="other">The other instance to compare against.</param>
        /// <returns>Whether online IDs match. If either instance is missing an online ID, this will return false.</returns>
        public static bool MatchesOnlineID(this IRulesetInfo? instance, IRulesetInfo? other) => matchesOnlineID(instance, other);

        /// <summary>
        /// Check whether the online ID of two <see cref="APIUser"/>s match.
        /// </summary>
        /// <param name="instance">The instance to compare.</param>
        /// <param name="other">The other instance to compare against.</param>
        /// <returns>Whether online IDs match. If either instance is missing an online ID, this will return false.</returns>
        public static bool MatchesOnlineID(this APIUser? instance, APIUser? other) => matchesOnlineID(instance, other);

        /// <summary>
        /// Check whether the online ID of two <see cref="IScoreInfo"/>s match.
        /// </summary>
        /// <param name="instance">The instance to compare.</param>
        /// <param name="other">The other instance to compare against.</param>
        /// <returns>
        /// Whether online IDs match.
        /// Both <see cref="IHasOnlineID{T}.OnlineID"/> and <see cref="IScoreInfo.LegacyOnlineID"/> are checked, in that order.
        /// If either instance is missing an online ID, this will return false.
        /// </returns>
        public static bool MatchesOnlineID(this IScoreInfo? instance, IScoreInfo? other)
        {
            if (matchesOnlineID(instance, other))
                return true;

            if (instance == null || other == null)
                return false;

            if (instance.LegacyOnlineID < 0 || other.LegacyOnlineID < 0)
                return false;

            return instance.LegacyOnlineID.Equals(other.LegacyOnlineID);
        }

        private static bool matchesOnlineID(this IHasOnlineID<long>? instance, IHasOnlineID<long>? other)
        {
            if (instance == null || other == null)
                return false;

            if (instance.OnlineID < 0 || other.OnlineID < 0)
                return false;

            return instance.OnlineID.Equals(other.OnlineID);
        }

        private static bool matchesOnlineID(this IHasOnlineID<int>? instance, IHasOnlineID<int>? other)
        {
            if (instance == null || other == null)
                return false;

            if (instance.OnlineID < 0 || other.OnlineID < 0)
                return false;

            return instance.OnlineID.Equals(other.OnlineID);
        }

        // intentionally chosen to match stable.
        // see https://referencesource.microsoft.com/#mscorlib/system/io/path.cs,88
        private static readonly char[] invalid_filename_chars =
        {
            '\"', '<', '>', '|', '\0', (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10, (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17,
            (char)18, (char)19, (char)20, (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30, (char)31, ':', '*', '?', '\\', '/'
        };

        /// <summary>
        /// Create a valid filename which should work across all platforms.
        /// </summary>
        /// <remarks>
        /// <para>
        /// We are using this in place of <see cref="Path.GetInvalidFileNameChars"/>
        /// as that function works per-platform, and therefore returns a different set of characters on different OSes.
        /// </para>
        /// <para>
        /// Note that the behaviour of this method is LOAD-BEARING for things such as interoperability of beatmap exports with stable,
        /// especially with respect to beatmap submission.
        /// DO NOT CHANGE THE SEMANTICS OF THIS METHOD unless you know well what you are doing.
        /// </para>
        /// </remarks>
        /// <seealso href="https://github.com/peppy/osu-stable-reference/blob/67795dba3c308e7d0493b296149dcb073ca47ecb/osu!common/Helpers/GeneralHelper.cs#L41-L46"/>
        public static string GetValidFilename(this string filename)
        {
            foreach (char c in invalid_filename_chars)
                filename = filename.Replace(c.ToString(), string.Empty);
            return filename;
        }

        public static bool RequiresSupporter(this BeatmapLeaderboardScope scope, bool filterMods)
        {
            switch (scope)
            {
                case BeatmapLeaderboardScope.Local:
                    return false;

                case BeatmapLeaderboardScope.Country:
                case BeatmapLeaderboardScope.Friend:
                    return true;
            }

            return filterMods;
        }
    }
}
