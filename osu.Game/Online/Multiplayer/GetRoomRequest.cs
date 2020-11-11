// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API;

namespace osu.Game.Online.Multiplayer
{
    public class GetRoomRequest : APIRequest<Room>
    {
        private readonly int roomId;

        public GetRoomRequest(int roomId)
        {
            this.roomId = roomId;
        }

        protected override string Target => $"rooms/{roomId}";
    }
}
