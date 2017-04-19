// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Game.Database;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Online.API.Requests
{
    public class GetScoresRequest : APIRequest<GetScoresResponse>
    {
        private readonly BeatmapInfo beatmap;

        public GetScoresRequest(BeatmapInfo beatmap)
        {
            this.beatmap = beatmap;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            //req.AddParameter(@"c", beatmap.Hash);
            //req.AddParameter(@"f", beatmap.Path);
            return req;
        }

        protected override string Target => $@"beatmaps/{beatmap.OnlineBeatmapID}/scores";
    }

    public class GetScoresResponse
    {
        [JsonProperty(@"scores")]
        public IEnumerable<Score> Scores;
    }
}