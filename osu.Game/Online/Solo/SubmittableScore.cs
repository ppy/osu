// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Online.Solo
{
    /// <summary>
    /// A class specifically for sending scores to the API during score submission.
    /// This is used instead of <see cref="APIScore"/> due to marginally different serialisation naming requirements.
    /// </summary>
    [Serializable]
    public class SubmittableScore
    {
        [JsonProperty("rank")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScoreRank Rank { get; set; }

        [JsonProperty("total_score")]
        public long TotalScore { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty(@"pp")]
        public double? PP { get; set; }

        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

        [JsonProperty("ruleset_id")]
        public int RulesetID { get; set; }

        [JsonProperty("passed")]
        public bool Passed { get; set; }

        // Used for API serialisation/deserialisation.
        [JsonProperty("mods")]
        public APIMod[] Mods { get; set; }

        [JsonProperty("user")]
        public APIUser User { get; set; }

        [JsonProperty("statistics")]
        public Dictionary<HitResult, int> Statistics { get; set; }

        [UsedImplicitly]
        public SubmittableScore()
        {
        }

        public SubmittableScore(ScoreInfo score)
        {
            Rank = score.Rank;
            TotalScore = score.TotalScore;
            Accuracy = score.Accuracy;
            PP = score.PP;
            MaxCombo = score.MaxCombo;
            RulesetID = score.RulesetID;
            Passed = score.Passed;
            Mods = score.APIMods;
            User = score.User;
            Statistics = score.Statistics;
        }
    }
}
