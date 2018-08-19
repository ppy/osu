// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
