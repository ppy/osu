// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using System.Collections.Generic;

namespace osu.Game.Online.API.Requests
{
    public class GetUserMostPlayedBeatmapsRequest : APIRequest<List<MostPlayedBeatmap>>
    {
        private readonly long userId;
        private readonly int offset;

        public GetUserMostPlayedBeatmapsRequest(long userId, int offset = 0)
        {
            this.userId = userId;
            this.offset = offset;
        }

        protected override string Target => $@"users/{userId}/beatmapsets/most_played?offset={offset}";
    }

    public class MostPlayedBeatmap
    {
        [JsonProperty("beatmap_id")]
        public int BeatmapID;

        [JsonProperty("count")]
        public int PlayCount;

        [JsonProperty]
        private BeatmapInfo beatmap;

        [JsonProperty]
        private APIResponseBeatmapSet beatmapSet;

        public BeatmapInfo GetBeatmapInfo(RulesetStore rulesets)
        {
            BeatmapSetInfo setInfo = beatmapSet.ToBeatmapSet(rulesets);
            beatmap.BeatmapSet = setInfo;
            beatmap.OnlineBeatmapSetID = setInfo.OnlineBeatmapSetID;
            beatmap.Metadata = setInfo.Metadata;
            return beatmap;
        }
    }
}
