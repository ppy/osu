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
        public int Combo { get; set; }

        public int MaxCombo { get; set; }

        public Dictionary<HitResult, int> Statistics { get; set; }

        /// <summary>
        /// Construct header summary information from a point-in-time reference to a score which is actively being played.
        /// </summary>
        /// <param name="score">The score for reference.</param>
        public FrameHeader(ScoreInfo score)
        {
            Combo = score.Combo;
            MaxCombo = score.MaxCombo;

            // copy for safety
            Statistics = new Dictionary<HitResult, int>(score.Statistics);
        }

        [JsonConstructor]
        public FrameHeader(int combo, int maxCombo, Dictionary<HitResult, int> statistics)
        {
            Combo = combo;
            MaxCombo = maxCombo;
            Statistics = statistics;
        }
    }
}
