// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Game.Scoring;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIRoomScoreInfo : ScoreInfo
    {
        [JsonProperty("attempts")]
        public int TotalAttempts { get; set; }

        [JsonProperty("completed")]
        public int CompletedBeatmaps { get; set; }
    }
}
