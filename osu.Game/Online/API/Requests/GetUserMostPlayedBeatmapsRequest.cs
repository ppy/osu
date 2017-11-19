// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Extensions;
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
        private BeatmapResponse beatmap;

        [JsonProperty]
        private GetBeatmapSetsResponse beatmapSet;

        public BeatmapInfo GetBeatmapInfo(RulesetStore rulesets)
        {
            BeatmapSetInfo setInfo = beatmapSet.ToBeatmapSet(rulesets);
            return new BeatmapInfo
            {
                OnlineBeatmapID = beatmap.Id,
                OnlineBeatmapSetID = setInfo.OnlineBeatmapSetID,
                Ruleset = rulesets.AvailableRulesets.FirstOrDefault(ruleset => ruleset.Name.Equals(beatmap.Mode)),
                StarDifficulty = beatmap.DifficultyRating,
                Version = beatmap.Version,
                Metadata = setInfo.Metadata,
                BeatmapSet = setInfo,
            };
        }

        private class BeatmapResponse
        {
            [JsonProperty]
            public int Id;

            [JsonProperty]
            public string Mode;

            [JsonProperty("difficulty_rating")]
            public double DifficultyRating;

            [JsonProperty]
            public string Version;
        }
    }
}
