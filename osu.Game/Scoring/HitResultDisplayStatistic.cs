// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Scoring
{
    /// <summary>
    /// Compiled result data for a specific <see cref="HitResult"/> in a score.
    /// </summary>
    public class HitResultDisplayStatistic
    {
        /// <summary>
        /// The associated result type.
        /// </summary>
        public HitResult Result { get; }

        /// <summary>
        /// The count of successful hits of this type.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// The maximum achievable hits of this type. May be null if undetermined.
        /// </summary>
        public int? MaxCount { get; }

        /// <summary>
        /// A custom display name for the result type. May be provided by rulesets to give better clarity.
        /// </summary>
        public string DisplayName { get; }

        public HitResultDisplayStatistic(HitResult result, int count, int? maxCount, string displayName)
        {
            Result = result;
            Count = count;
            MaxCount = maxCount;
            DisplayName = displayName;
        }
    }
}
