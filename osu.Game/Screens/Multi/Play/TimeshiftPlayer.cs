// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Net.Http;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.IO.Network;
using osu.Game.Online.API;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.Multi.Play
{
    public class TimeshiftPlayer : Player
    {
        private readonly int roomId;
        private readonly int playlistItemId;

        [Resolved]
        private APIAccess api { get; set; }

        public TimeshiftPlayer(int roomId, int playlistItemId)
        {
            this.roomId = roomId;
            this.playlistItemId = playlistItemId;
        }

        private int token;

        [BackgroundDependencyLoader]
        private void load()
        {
            var req = new CreateScoreRequest(roomId, playlistItemId);
            req.Success += r => token = r.ID;
            req.Failure += e => { };
            api.Queue(req);
        }

        protected override ScoreInfo CreateScore()
        {
            var score = base.CreateScore();

            var request = new SubmitScoreRequest(token, roomId, playlistItemId, score);
            request.Success += () => { };
            request.Failure += e => { };
            api.Queue(request);

            return score;
        }
    }

    public class SubmitScoreRequest : APIRequest
    {
        private readonly int scoreId;
        private readonly int roomId;
        private readonly int playlistItemId;
        private readonly ScoreInfo scoreInfo;

        public SubmitScoreRequest(int scoreId, int roomId, int playlistItemId, ScoreInfo scoreInfo)
        {
            this.scoreId = scoreId;
            this.roomId = roomId;
            this.playlistItemId = playlistItemId;
            this.scoreInfo = scoreInfo;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.ContentType = "application/json";
            req.Method = HttpMethod.Put;

            req.AddRaw(JsonConvert.SerializeObject(scoreInfo));

            return req;
        }

        protected override string Target => $@"rooms/{roomId}/playlist/{playlistItemId}/scores/{scoreId}";
    }

    public class CreateScoreRequest : APIRequest<CreateScoreResult>
    {
        private readonly int roomId;
        private readonly int playlistItemId;

        public CreateScoreRequest(int roomId, int playlistItemId)
        {
            this.roomId = roomId;
            this.playlistItemId = playlistItemId;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Post;
            return req;
        }

        protected override string Target => $@"rooms/{roomId}/playlist/{playlistItemId}/scores";
    }

    public class CreateScoreResult
    {
        [JsonProperty("id")]
        public int ID { get; set; }
    }
}
