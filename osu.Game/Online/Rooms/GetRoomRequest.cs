// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API;

namespace osu.Game.Online.Rooms
{
    public class GetRoomRequest : APIRequest<Room>
    {
        public readonly int RoomId;

        public GetRoomRequest(int roomId)
        {
            RoomId = roomId;
        }

        protected override string Target => $"rooms/{RoomId}";
    }
}
