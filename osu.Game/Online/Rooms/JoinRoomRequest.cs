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
        private readonly string password;

        public JoinRoomRequest(Room room, string password)
        {
            this.room = room;
            this.password = password;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Put;
            req.AddParameter("password", password);
            return req;
        }

        protected override string Target => $"rooms/{room.RoomID.Value}/users/{User.Id}";
    }
}
