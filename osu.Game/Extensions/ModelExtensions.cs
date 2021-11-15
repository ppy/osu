// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Extensions
{
    public static class ModelExtensions
    {
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
        public static string GetDisplayString(this object model)
        {
            string result = null;

            switch (model)
            {
                case IBeatmapSetInfo beatmapSetInfo:
                    result = beatmapSetInfo.Metadata?.GetDisplayTitle();
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
        /// Check whether the online ID of two instances match.
        /// </summary>
        /// <param name="instance">The instance to compare.</param>
        /// <param name="other">The other instance to compare against.</param>
        /// <returns>Whether online IDs match. If either instance is missing an online ID, this will return false.</returns>
        public static bool MatchesOnlineID(this IHasOnlineID<long> instance, IHasOnlineID<long> other)
        {
            if (instance.OnlineID < 0 || other.OnlineID < 0)
                return false;

            return instance.OnlineID.Equals(other.OnlineID);
        }

        /// <summary>
        /// Check whether the online ID of two instances match.
        /// </summary>
        /// <param name="instance">The instance to compare.</param>
        /// <param name="other">The other instance to compare against.</param>
        /// <returns>Whether online IDs match. If either instance is missing an online ID, this will return false.</returns>
        public static bool MatchesOnlineID(this IHasOnlineID<int> instance, IHasOnlineID<int> other)
        {
            if (instance.OnlineID < 0 || other.OnlineID < 0)
                return false;

            return instance.OnlineID.Equals(other.OnlineID);
        }
    }
}
