// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.Matchmaking
{
    public class MatchmakingUserStats
    {
        /// <summary>
        /// The user's current rating.
        /// </summary>
        public int Rating { get; }

        /// <summary>
        /// The user's current sigma (rank uncertainty) value.
        /// </summary>
        public double Sigma { get; }

        /// <summary>
        /// The user's current rank. <c>null</c> if the current rating is provisional.
        /// </summary>
        public int? Rank { get; }

        public MatchmakingUserStats(int rating, double sigma, int? rank)
        {
            Rating = rating;
            Sigma = sigma;
            Rank = rank;
        }
    }
}
