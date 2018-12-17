// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.IO.Network;
using osu.Framework.Logging;
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

        private int? token;

        [BackgroundDependencyLoader]
        private void load()
        {
            token = null;

            bool failed = false;

            var req = new CreateScoreRequest(roomId, playlistItemId);
            req.Success += r => token = r.ID;
            req.Failure += e =>
            {
                failed = true;

                Logger.Error(e, "Failed to retrieve a score submission token.");

                Schedule(() =>
                {
                    ValidForResume = false;
                    Exit();
                });
            };

            api.Queue(req);

            while (!failed && !token.HasValue)
                Thread.Sleep(1000);
        }

        protected override ScoreInfo CreateScore()
        {
            var score = base.CreateScore();

            Debug.Assert(token != null);

            var request = new SubmitScoreRequest(token.Value, roomId, playlistItemId, score);
            request.Failure += e => Logger.Error(e, "Failed to submit score");
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
