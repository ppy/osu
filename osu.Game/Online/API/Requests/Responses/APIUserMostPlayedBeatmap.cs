// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIUserMostPlayedBeatmap
    {
        [JsonProperty("beatmap_id")]
        public int BeatmapID { get; set; }

        [JsonProperty("count")]
        public int PlayCount { get; set; }

#pragma warning disable CA1507 // Happens to name the same because of casing preference
        [JsonProperty("beatmap")]
#pragma warning restore CA1507
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
