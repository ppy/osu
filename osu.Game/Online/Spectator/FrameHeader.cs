// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

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

        /// <summary>
        /// Construct header summary information from a point-in-time reference to a score which is actively being played.
        /// </summary>
        /// <param name="score">The score for reference.</param>
        public FrameHeader(ScoreInfo score)
        {
            Combo = score.Combo;
            MaxCombo = score.MaxCombo;
            Accuracy = score.Accuracy;

            // copy for safety
            Statistics = new Dictionary<HitResult, int>(score.Statistics);
        }

        [JsonConstructor]
        [SerializationConstructor]
        public FrameHeader(double accuracy, int combo, int maxCombo, Dictionary<HitResult, int> statistics, DateTimeOffset receivedTime)
        {
            Combo = combo;
            MaxCombo = maxCombo;
            Accuracy = accuracy;
            Statistics = statistics;
            ReceivedTime = receivedTime;
        }
    }
}
