// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API;

namespace osu.Game.Online.Rooms
{
    public static class RoomExtensions
    {
        /// <summary>
        /// Get the room page URL, or <c>null</c> if unavailable.
        /// </summary>
        public static string? GetOnlineURL(this Room room, IAPIProvider api)
        {
            if (!room.RoomID.HasValue)
                return null;

            return $@"{api.Endpoints.WebsiteUrl}/multiplayer/rooms/{room.RoomID.Value}";
        }
    }
}
