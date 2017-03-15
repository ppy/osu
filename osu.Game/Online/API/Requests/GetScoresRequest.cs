using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Game.Database;
using osu.Game.Modes;

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
            req.AddParameter(@"c", beatmap.Hash);
            req.AddParameter(@"f", beatmap.Path);
            return req;
        }

        protected override string Target => @"beatmaps/scores";
    }

    public class GetScoresResponse
    {
        [JsonProperty(@"beatmap")]
        public BeatmapInfo Beatmap;

        [JsonProperty(@"scores")]
        public IEnumerable<Score> Scores;
    }
}