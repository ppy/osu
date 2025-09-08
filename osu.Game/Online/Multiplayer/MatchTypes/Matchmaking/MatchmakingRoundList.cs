// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using MessagePack;

namespace osu.Game.Online.Multiplayer.MatchTypes.Matchmaking
{
    /// <summary>
    /// Describes the per-round scores of a matchmaking room.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public class MatchmakingRoundList : IEnumerable<MatchmakingRound>
    {
        /// <summary>
        /// A key-value-pair mapping of rounds to scores.
        /// </summary>
        [Key(0)]
        public IDictionary<int, MatchmakingRound> RoundsDictionary { get; set; } = new Dictionary<int, MatchmakingRound>();

        /// <summary>
        /// Creates or retrieves the score for the given round.
        /// </summary>
        /// <param name="round">The round.</param>
        public MatchmakingRound this[int round]
        {
            get
            {
                if (RoundsDictionary.TryGetValue(round, out MatchmakingRound? score))
                    return score;

                return RoundsDictionary[round] = new MatchmakingRound { Round = round };
            }
        }

        /// <summary>
        /// The total number of rounds.
        /// </summary>
        [IgnoreMember]
        public int Count => RoundsDictionary.Count;

        public IEnumerator<MatchmakingRound> GetEnumerator() => RoundsDictionary.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
