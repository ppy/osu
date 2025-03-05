// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Lounge
{
    public interface IOnlinePlayLounge
    {
        /// <summary>
        /// Attempts to join the given room.
        /// </summary>
        /// <param name="room">The room to join.</param>
        /// <param name="password">The password.</param>
        /// <param name="onSuccess">A delegate to invoke if the user joined the room.</param>
        /// <param name="onFailure">A delegate to invoke if the user is not able join the room.</param>
        void Join(Room room, string? password, Action<Room>? onSuccess = null, Action<string>? onFailure = null);

        /// <summary>
        /// Copies the given room and opens it as a fresh (not-yet-created) one.
        /// </summary>
        /// <param name="room">The room to copy.</param>
        void OpenCopy(Room room);

        /// <summary>
        /// Closes the given room.
        /// </summary>
        /// <param name="room">The room to close.</param>
        void Close(Room room);
    }
}
