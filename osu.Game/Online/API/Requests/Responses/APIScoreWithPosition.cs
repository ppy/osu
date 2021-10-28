// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIScoreWithPosition
    {
        [JsonProperty(@"position")]
        public int? Position;

        [JsonProperty(@"score")]
        public APIScoreInfo Score;

        public ScoreInfo CreateScoreInfo(RulesetStore rulesets)

        {
            var score = Score.CreateScoreInfo(rulesets);
            score.Position = Position;
            return score;
        }
    }
}
