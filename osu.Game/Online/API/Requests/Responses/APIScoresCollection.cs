// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIScoresCollection
    {
        [JsonProperty(@"scores")]
        public List<APIScoreInfo> Scores;

        [JsonProperty(@"userScore")]
        public APIScoreWithPosition UserScore;
    }
}
