// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.Multiplayer;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests
{
    public class JoinRoomRequest : APIRequest
    {
        private readonly Room room;
        private readonly User user;

        public JoinRoomRequest(Room room, User user)
        {
            this.room = room;
            this.user = user;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Put;
            return req;
        }

        protected override string Target => $"rooms/{room.RoomID.Value}/users/{user.Id}";
    }
}
