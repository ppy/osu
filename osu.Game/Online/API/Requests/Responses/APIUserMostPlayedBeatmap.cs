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
        public int BeatmapID;

        [JsonProperty("count")]
        public int PlayCount;

        [JsonProperty]
        private BeatmapInfo beatmap;

        [JsonProperty]
        private APIBeatmapSet beatmapSet;

        public BeatmapInfo GetBeatmapInfo(RulesetStore rulesets)
        {
            BeatmapSetInfo setInfo = beatmapSet.ToBeatmapSet(rulesets);
            beatmap.BeatmapSet = setInfo;
            beatmap.Metadata = setInfo.Metadata;
            return beatmap;
        }
    }
}
