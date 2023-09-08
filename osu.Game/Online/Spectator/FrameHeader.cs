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
        /// The total score.
        /// </summary>
        [Key(0)]
        public long TotalScore { get; set; }

        /// <summary>
        /// The current accuracy of the score.
        /// </summary>
        [Key(1)]
        public double Accuracy { get; set; }

        /// <summary>
        /// The current combo of the score.
        /// </summary>
        [Key(2)]
        public int Combo { get; set; }

        /// <summary>
        /// The maximum combo achieved up to the current point in time.
        /// </summary>
        [Key(3)]
        public int MaxCombo { get; set; }

        /// <summary>
        /// Cumulative hit statistics.
        /// </summary>
        [Key(4)]
        public Dictionary<HitResult, int> Statistics { get; set; }

        /// <summary>
        /// Additional statistics that guides the score processor to calculate the correct score for this frame.
        /// </summary>
        [Key(5)]
        public ScoreProcessorStatistics ScoreProcessorStatistics { get; set; }

        /// <summary>
        /// The time at which this frame was received by the server.
        /// </summary>
        [Key(6)]
        public DateTimeOffset ReceivedTime { get; set; }

        /// <summary>
        /// Construct header summary information from a point-in-time reference to a score which is actively being played.
        /// </summary>
        /// <param name="score">The score for reference.</param>
        /// <param name="statistics">The score processor statistics for the current point in time.</param>
        public FrameHeader(ScoreInfo score, ScoreProcessorStatistics statistics)
        {
            TotalScore = score.TotalScore;
            Accuracy = score.Accuracy;
            Combo = score.Combo;
            MaxCombo = score.MaxCombo;
            // copy for safety
            Statistics = new Dictionary<HitResult, int>(score.Statistics);

            ScoreProcessorStatistics = statistics;
        }

        [JsonConstructor]
        [SerializationConstructor]
        public FrameHeader(long totalScore, double accuracy, int combo, int maxCombo, Dictionary<HitResult, int> statistics, ScoreProcessorStatistics scoreProcessorStatistics, DateTimeOffset receivedTime)
        {
            TotalScore = totalScore;
            Accuracy = accuracy;
            Combo = combo;
            MaxCombo = maxCombo;
            Statistics = statistics;
            ScoreProcessorStatistics = scoreProcessorStatistics;
            ReceivedTime = receivedTime;
        }
    }
}
