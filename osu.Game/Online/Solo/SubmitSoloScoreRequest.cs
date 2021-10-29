// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.IO.Network;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Online.Solo
{
    public class SubmitSoloScoreRequest : APIRequest<MultiplayerScore>
    {
        private readonly long scoreId;

        private readonly int beatmapId;

        private readonly SubmittableScore score;

        public SubmitSoloScoreRequest(int beatmapId, long scoreId, ScoreInfo scoreInfo)
        {
            this.beatmapId = beatmapId;
            this.scoreId = scoreId;
            score = new SubmittableScore(scoreInfo);
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.ContentType = "application/json";
            req.Method = HttpMethod.Put;

            req.AddRaw(JsonConvert.SerializeObject(score, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            return req;
        }

        protected override string Target => $@"beatmaps/{beatmapId}/solo/scores/{scoreId}";
    }

    /// <summary>
    /// A class specifically for sending scores to the API during score submission.
    /// This is used instead of <see cref="APIScoreInfo"/> due to marginally different serialisation naming requirements.
    /// </summary>
    public class SubmittableScore
    {
        [JsonProperty("rank")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScoreRank Rank { get; }

        [JsonProperty("total_score")]
        public long TotalScore { get; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; }

        [JsonProperty(@"pp")]
        public double? PP { get; }

        [JsonProperty("max_combo")]
        public int MaxCombo { get; }

        [JsonProperty("ruleset_id")]
        public int RulesetID { get; }

        [JsonProperty("passed")]
        public bool Passed { get; }

        // Used for API serialisation/deserialisation.
        [JsonProperty("mods")]
        public APIMod[] Mods { get; }

        [JsonProperty("user")]
        public User User { get; }

        [JsonProperty("statistics")]
        public Dictionary<HitResult, int> Statistics { get; }

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
