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

            // Degenerate case: prefer players that have participated in more rounds.
            int compare = y.Rounds.Count.CompareTo(x.Rounds.Count);
            if (compare != 0)
                return compare;

            // Base case: players with more points win the match.
            compare = y.Points.CompareTo(x.Points);
            if (compare != 0)
                return compare;

            // Tiebreaker 1: prefer players who won in earlier rounds.
            for (int r = 1; r <= rounds; r++)
            {
                x.Rounds.RoundsDictionary.TryGetValue(r, out var xRound);
                y.Rounds.RoundsDictionary.TryGetValue(r, out var yRound);

                if (xRound == null && yRound == null)
                    continue;

                if (xRound == null)
                    return 1;

                if (yRound == null)
                    return -1;

                compare = xRound.Placement.CompareTo(yRound.Placement);
                if (compare != 0)
                    return compare;
            }

            // Tiebreaker 2: all users have the same placement across all rounds.
            return x.UserId.CompareTo(y.UserId);
        }
    }
}
