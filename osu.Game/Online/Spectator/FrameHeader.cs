// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Online.Spectator
{
    [Serializable]
    public class FrameHeader
    {
        /// <summary>
        /// The current accuracy of the score.
        /// </summary>
        public double Accuracy { get; set; }

        /// <summary>
        /// The current combo of the score.
        /// </summary>
        public int Combo { get; set; }

        /// <summary>
        /// The maximum combo achieved up to the current point in time.
        /// </summary>
        public int MaxCombo { get; set; }

        /// <summary>
        /// Cumulative hit statistics.
        /// </summary>
        public Dictionary<HitResult, int> Statistics { get; set; }

        /// <summary>
        /// The time at which this frame was received by the server.
        /// </summary>
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
        public FrameHeader(int combo, int maxCombo, double accuracy, Dictionary<HitResult, int> statistics, DateTimeOffset receivedTime)
        {
            Combo = combo;
            MaxCombo = maxCombo;
            Accuracy = accuracy;
            Statistics = statistics;
            ReceivedTime = receivedTime;
        }
    }
}
