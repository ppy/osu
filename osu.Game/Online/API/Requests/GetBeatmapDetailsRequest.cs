// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Game.Database;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapDetailsRequest : APIRequest<GetBeatmapDeatilsResponse>
    {
        private readonly BeatmapInfo beatmap;

        private string lookupString;

        public GetBeatmapDetailsRequest(BeatmapInfo beatmap)
        {
            this.beatmap = beatmap;
        }

        protected override WebRequest CreateWebRequest()
        {
            if (beatmap.OnlineBeatmapID > 0)
                lookupString = beatmap.OnlineBeatmapID.ToString();
            else
                lookupString = $@"lookup?checksum={beatmap.Hash}&filename={beatmap.Path}";

            var req = base.CreateWebRequest();

            return req;
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
                this.Fails = value.Fails;
                this.Retries = value.Retries;
            }
        }

        //and other metrics in the beatmap set.
        [JsonProperty(@"beatmapset")]
        private BeatmapMetrics beatmapSet
        {
            set
            {
                this.Ratings = value.Ratings;
            }
        }
    }
}
