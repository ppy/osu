// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.API;

namespace osu.Game.Online.Rooms
{
    public class JoinRoomRequest : APIRequest
    {
        public readonly Room Room;
        public readonly string Password;

        public JoinRoomRequest(Room room, string password)
        {
            Room = room;
            Password = password;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Put;
            return req;
        }

        // Todo: Password needs to be specified here rather than via AddParameter() because this is a PUT request. May be a framework bug.
        protected override string Target => $"rooms/{Room.RoomID.Value}/users/{User.Id}?password={Password}";
    }
}
