// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Net.Http;
using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Online.API.Requests
{
    public class CreateRoomRequest : APIRequest<APICreatedRoom>
    {
        private readonly Room room;

        public CreateRoomRequest(Room room)
        {
            this.room = room;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.ContentType = "application/json";
            req.Method = HttpMethod.Post;

            req.AddRaw(JsonConvert.SerializeObject(room));

            return req;
        }

        protected override string Target => "rooms";
    }
}
