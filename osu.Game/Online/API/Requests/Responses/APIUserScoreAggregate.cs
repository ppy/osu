// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIUserScoreAggregate
    {
        [JsonProperty("attempts")]
        public int TotalAttempts { get; set; }

        [JsonProperty("completed")]
        public int CompletedBeatmaps { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty(@"pp")]
        public double? PP { get; set; }

        [JsonProperty(@"room_id")]
        public long RoomID { get; set; }

        [JsonProperty("total_score")]
        public long TotalScore { get; set; }

        [JsonProperty(@"user_id")]
        public long UserID { get; set; }

        [JsonProperty("user")]
        public APIUser User { get; set; }

        [JsonProperty("position")]
        public int? Position { get; set; }

        public ScoreInfo CreateScoreInfo() =>
            new ScoreInfo
            {
                Accuracy = Accuracy,
                PP = PP,
                TotalScore = TotalScore,
                User = User,
                Position = Position,
                Mods = Array.Empty<Mod>()
            };
    }
}
