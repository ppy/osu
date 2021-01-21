// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Game.Online.API;
using osu.Game.Scoring;

namespace osu.Game.Online.Rooms
{
    public class SubmitRoomScoreRequest : APIRequest<MultiplayerScore>
    {
        private readonly int scoreId;
        private readonly int roomId;
        private readonly int playlistItemId;
        private readonly ScoreInfo scoreInfo;

        public SubmitRoomScoreRequest(int scoreId, int roomId, int playlistItemId, ScoreInfo scoreInfo)
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

            req.AddRaw(JsonConvert.SerializeObject(scoreInfo, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            return req;
        }

        protected override string Target => $@"rooms/{roomId}/playlist/{playlistItemId}/scores/{scoreId}";
    }
}
