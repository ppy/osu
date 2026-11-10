// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.Matchmaking
{
    public class MatchmakingUserStats
    {
        // Taken from https://github.com/ppy/osu-web/blob/8eb0994a64af8d68e365b487d35be24b2daff75a/app/Models/MatchmakingUserStats.php#L27.
        public const int MIN_SIGMA_PROVISIONAL = 100;

        public int Rating { get; }
        public double Sigma { get; }
        public int Rank { get; }

        public bool IsRatingProvisional => Sigma >= MIN_SIGMA_PROVISIONAL;

        public MatchmakingUserStats(int rating, double sigma, int rank)
        {
            Rating = rating;
            Sigma = sigma;
            Rank = rank;
        }
    }
}
