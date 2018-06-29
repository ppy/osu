// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;
using osu.Game.Beatmaps;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIBeatmapMetrics : BeatmapMetrics
    {
        //the online API returns some metrics as a nested object.
        [JsonProperty(@"failtimes")]
        private BeatmapMetrics failTimes
        {
            set
            {
                Fails = value.Fails;
                Retries = value.Retries;
            }
        }

        //and other metrics in the beatmap set.
        [JsonProperty(@"beatmapset")]
        private BeatmapMetrics beatmapSet
        {
            set => Ratings = value.Ratings;
        }
    }
}
