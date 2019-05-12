// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIUserMostPlayedBeatmap
    {
        [JsonProperty("beatmap_id")]
        public int BeatmapID { get; set; }

        [JsonProperty("count")]
        public int PlayCount { get; set; }

        [JsonProperty]
        private BeatmapInfo beatmap { get; set; }

        [JsonProperty]
        private APIBeatmapSet beatmapSet { get; set; }

        public BeatmapInfo GetBeatmapInfo(RulesetStore rulesets)
        {
            BeatmapSetInfo setInfo = beatmapSet.ToBeatmapSet(rulesets);
            beatmap.BeatmapSet = setInfo;
            beatmap.Metadata = setInfo.Metadata;
            return beatmap;
        }
    }
}
