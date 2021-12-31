// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Users;

#nullable enable

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
        /// <returns>Whether online IDs match. If either instance is missing an online ID, this will return false.</returns>
        public static bool MatchesOnlineID(this IScoreInfo? instance, IScoreInfo? other) => matchesOnlineID(instance, other);

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

        private static readonly char[] invalid_filename_characters = Path.GetInvalidFileNameChars()
                                                                         // Backslash is added to avoid issues when exporting to zip.
                                                                         // See SharpCompress filename normalisation https://github.com/adamhathcock/sharpcompress/blob/a1e7c0068db814c9aa78d86a94ccd1c761af74bd/src/SharpCompress/Writers/Zip/ZipWriter.cs#L143.
                                                                         .Append('\\')
                                                                         .ToArray();

        /// <summary>
        /// Get a valid filename for use inside a zip file. Avoids backslashes being incorrectly converted to directories.
        /// </summary>
        public static string GetValidArchiveContentFilename(this string filename)
        {
            foreach (char c in invalid_filename_characters)
                filename = filename.Replace(c, '_');
            return filename;
        }
    }
}
