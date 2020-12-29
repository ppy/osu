// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.API;

namespace osu.Game.Online.Rooms
{
    public class JoinRoomRequest : APIRequest
    {
        private readonly Room room;

        public JoinRoomRequest(Room room)
        {
            this.room = room;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Put;
            return req;
        }

        protected override string Target => $"rooms/{room.RoomID.Value}/users/{User.Id}";
    }
}
