// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using MessagePack;
using Newtonsoft.Json;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Online.Spectator
{
    [Serializable]
    [MessagePackObject]
    public class FrameHeader
    {
        /// <summary>
        /// The current accuracy of the score.
        /// </summary>
        [Key(0)]
        public double Accuracy { get; set; }

        /// <summary>
        /// The current combo of the score.
        /// </summary>
        [Key(1)]
        public int Combo { get; set; }

        /// <summary>
        /// The maximum combo achieved up to the current point in time.
        /// </summary>
        [Key(2)]
        public int MaxCombo { get; set; }

        /// <summary>
        /// Cumulative hit statistics.
        /// </summary>
        [Key(3)]
        public Dictionary<HitResult, int> Statistics { get; set; }

        /// <summary>
        /// The time at which this frame was received by the server.
        /// </summary>
        [Key(4)]
        public DateTimeOffset ReceivedTime { get; set; }

        [Key(5)]
        public double? BaseScore { get; set; }

        [Key(6)]
        public double? BonusScore { get; set; }

        [Key(7)]
        public ScoringValues? Maximum { get; set; }

        /// <summary>
        /// Construct header summary information from a point-in-time reference to a score which is actively being played.
        /// </summary>
        /// <param name="score">The score for reference.</param>
        public FrameHeader(ScoreInfo score)
        {
            BaseScore = score.BaseScore;
            BonusScore = score.BonusScore;
            Accuracy = score.Accuracy;
            Combo = score.Combo;
            MaxCombo = score.MaxCombo;

            // copy for safety
            Statistics = new Dictionary<HitResult, int>(score.Statistics);
        }

        [JsonConstructor]
        [SerializationConstructor]
        public FrameHeader(int baseScore, int bonusScore, double accuracy, int combo, int maxCombo, Dictionary<HitResult, int> statistics, DateTimeOffset receivedTime)
        {
            BaseScore = baseScore;
            BonusScore = bonusScore;
            Accuracy = accuracy;
            Combo = combo;
            MaxCombo = maxCombo;
            Statistics = statistics;
            ReceivedTime = receivedTime;
        }

        // this is supposed to be for messagepack as a fallback constructor when baseScore and bonusScore is not supplied, but I don't know if this works.
        public FrameHeader(double accuracy, int combo, int maxCombo, Dictionary<HitResult, int> statistics, DateTimeOffset receivedTime)
        {
            Accuracy = accuracy;
            Combo = combo;
            MaxCombo = maxCombo;
            Statistics = statistics;
            ReceivedTime = receivedTime;
        }
    }
}
