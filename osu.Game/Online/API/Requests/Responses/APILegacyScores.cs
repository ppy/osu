// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APILegacyScores
    {
        [JsonProperty(@"scores")]
        public List<APILegacyScoreInfo> Scores;

        [JsonProperty(@"userScore")]
        public APILegacyUserTopScoreInfo UserScore;
    }

    public class APILegacyUserTopScoreInfo
    {
        [JsonProperty(@"position")]
        public int? Position;

        [JsonProperty(@"score")]
        public APILegacyScoreInfo Score;

        public ScoreInfo CreateScoreInfo(RulesetStore rulesets)
        {
            var score = Score.CreateScoreInfo(rulesets);
            score.Position = Position;
            return score;
        }
    }
}
