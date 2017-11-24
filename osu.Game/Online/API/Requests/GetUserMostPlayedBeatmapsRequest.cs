// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class GetUserMostPlayedBeatmapsRequest : GetUserBeatmapsRequest<UserMostPlayedBeatmapsResponse>
    {
        public GetUserMostPlayedBeatmapsRequest(long userID, BeatmapSetType type, int offset = 0)
            : base(userID, type, offset)
        {
            if (type != BeatmapSetType.MostPlayed)
                throw new ArgumentException("Please use " + nameof(GetUserBeatmapsRequest) + " instead");
        }
    }

    public class UserMostPlayedBeatmapsResponse
    {
        [JsonProperty("beatmap_id")]
        public int BeatmapID;

        [JsonProperty("count")]
        public int PlayCount;

        [JsonProperty]
        private BeatmapInfo beatmap;

        [JsonProperty]
        private GetBeatmapSetsResponse beatmapSet;

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
