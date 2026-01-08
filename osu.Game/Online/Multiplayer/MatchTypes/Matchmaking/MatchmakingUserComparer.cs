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

            int compare = compareAbandonedAt(x, y);
            if (compare != 0)
                return compare;

            compare = comparePoints(x, y);
            if (compare != 0)
                return compare;

            compare = compareRoundPlacements(x, y);
            if (compare != 0)
                return compare;

            return compareUserIds(x, y);
        }

        private int compareAbandonedAt(MatchmakingUser x, MatchmakingUser y)
        {
            DateTimeOffset xAbandonedAt = x.AbandonedAt ?? DateTimeOffset.MaxValue;
            DateTimeOffset yAbandonedAt = y.AbandonedAt ?? DateTimeOffset.MaxValue;
            return -xAbandonedAt.CompareTo(yAbandonedAt);
        }

        private int comparePoints(MatchmakingUser x, MatchmakingUser y)
        {
            return -x.Points.CompareTo(y.Points);
        }

        private int compareRoundPlacements(MatchmakingUser x, MatchmakingUser y)
        {
            for (int r = 1; r <= rounds; r++)
            {
                x.Rounds.RoundsDictionary.TryGetValue(r, out var xRound);
                y.Rounds.RoundsDictionary.TryGetValue(r, out var yRound);

                int xPlacement = xRound?.Placement ?? int.MaxValue;
                int yPlacement = yRound?.Placement ?? int.MaxValue;

                int compare = xPlacement.CompareTo(yPlacement);
                if (compare != 0)
                    return compare;
            }

            return 0;
        }

        private int compareUserIds(MatchmakingUser x, MatchmakingUser y)
        {
            return x.UserId.CompareTo(y.UserId);
        }
    }
}
