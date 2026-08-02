// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using MessagePack;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Online.Multiplayer.MatchTypes.Matchmaking
{
    /// <summary>
    /// Describes a user's score for a round of a matchmaking room.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public class MatchmakingRound
    {
        /// <summary>
        /// The round.
        /// </summary>
        [Key(0)]
        public required int Round { get; set; }

        /// <summary>
        /// The user's placement in this round (1-based).
        /// </summary>
        [Key(1)]
        public int Placement { get; set; }

        /// <summary>
        /// The achieved total score.
        /// </summary>
        [Key(2)]
        public long TotalScore { get; set; }

        /// <summary>
        /// The achieved accuracy.
        /// </summary>
        [Key(3)]
        public double Accuracy { get; set; }

        /// <summary>
        /// The achieved maximum combo.
        /// </summary>
        [Key(4)]
        public int MaxCombo { get; set; }

        /// <summary>
        /// The achieved score statistics.
        /// </summary>
        [Key(5)]
        public IDictionary<HitResult, int> Statistics { get; set; } = new Dictionary<HitResult, int>();
    }
}
