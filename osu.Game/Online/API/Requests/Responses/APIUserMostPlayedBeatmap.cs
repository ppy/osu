// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIUserMostPlayedBeatmap
    {
        [JsonProperty("beatmap_id")]
        public int BeatmapID { get; set; }

        [JsonProperty("count")]
        public int PlayCount { get; set; }

        [JsonProperty("beatmap")]
        private APIBeatmap beatmap { get; set; }

        public APIBeatmap BeatmapInfo
        {
            get
            {
                // old osu-web code doesn't nest set.
                beatmap.BeatmapSet = BeatmapSet;
                return beatmap;
            }
        }

        [JsonProperty("beatmapset")]
        public APIBeatmapSet BeatmapSet { get; set; }
    }
}
