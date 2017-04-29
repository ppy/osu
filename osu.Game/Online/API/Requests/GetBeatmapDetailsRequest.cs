// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;
using osu.Game.Database;

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapDetailsRequest : APIRequest<GetBeatmapDeatilsResponse>
    {
        private readonly BeatmapInfo beatmap;

        private string lookupString => beatmap.OnlineBeatmapID > 0 ? beatmap.OnlineBeatmapID.ToString() : $@"lookup?checksum={beatmap.Hash}&filename={beatmap.Path}";

        public GetBeatmapDetailsRequest(BeatmapInfo beatmap)
        {
            this.beatmap = beatmap;
        }

        protected override string Target => $@"beatmaps/{lookupString}";
    }

    public class GetBeatmapDeatilsResponse : BeatmapMetrics
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
            set
            {
                Ratings = value.Ratings;
            }
        }
    }
}
