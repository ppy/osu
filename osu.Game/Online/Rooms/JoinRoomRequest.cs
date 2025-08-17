// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.API;

namespace osu.Game.Online.Rooms
{
    public class JoinRoomRequest : APIRequest<Room>
    {
        public readonly Room Room;
        public readonly string? Password;

        public JoinRoomRequest(Room room, string? password)
        {
            Room = room;
            Password = password;

            // Also copy back to the source model, since it is likely to have been stored elsewhere.
            Success += r => Room.CopyFrom(r);
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Put;
            if (!string.IsNullOrEmpty(Password))
                req.AddParameter(@"password", Password, RequestParameterType.Query);
            return req;
        }

        protected override string Target => $@"rooms/{Room.RoomID}/users/{User!.Id}";
    }
}
