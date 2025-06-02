// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIScoresCollection
    {
        [JsonProperty(@"score_count")]
        public int ScoresCount;

        [JsonProperty(@"scores")]
        public List<SoloScoreInfo> Scores;

        [JsonProperty(@"user_score")]
        public APIScoreWithPosition UserScore;
    }
}
