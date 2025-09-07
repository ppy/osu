// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Online.Multiplayer.MatchTypes.Matchmaking
{
    /// <summary>
    /// Orders <see cref="MatchmakingUser"/> in order of placement.
    /// </summary>
    public class MatchmakingUserComparer : Comparer<MatchmakingUser>
    {
        private readonly int rounds;

        public MatchmakingUserComparer(int rounds)
        {
            this.rounds = rounds;
        }

        public override int Compare(MatchmakingUser? x, MatchmakingUser? y)
        {
            ArgumentNullException.ThrowIfNull(x);
            ArgumentNullException.ThrowIfNull(y);

            // X appears earlier in the list if it has more points.
            if (x.Points > y.Points)
                return -1;

            // Y appears earlier in the list if it has more points.
            if (y.Points > x.Points)
                return 1;

            // Tiebreaker 1 (likely): From each user's point-of-view, their earliest and best placement.
            for (int r = 1; r <= rounds; r++)
            {
                MatchmakingRound? xRound;
                x.Rounds.RoundsDictionary.TryGetValue(r, out xRound);

                MatchmakingRound? yRound;
                y.Rounds.RoundsDictionary.TryGetValue(r, out yRound);

                // Nothing to do if both players haven't played this round.
                if (xRound == null && yRound == null)
                    continue;

                // X appears later in the list if it hasn't played this round.
                if (xRound == null)
                    return 1;

                // Y appears later in the list if it hasn't played this round.
                if (yRound == null)
                    return -1;

                // X appears earlier in the list if it has a better placement in the round.
                int compare = xRound.Placement.CompareTo(yRound.Placement);
                if (compare != 0)
                    return compare;
            }

            // Tiebreaker 2 (unlikely): User ID.
            return x.UserId.CompareTo(y.UserId);
        }
    }
}
