// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
