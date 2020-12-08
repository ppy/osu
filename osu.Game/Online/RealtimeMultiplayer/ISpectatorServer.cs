// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;

namespace osu.Game.Online.RealtimeMultiplayer
{
    /// <summary>
    /// An interface defining the spectator server instance.
    /// </summary>
    public interface IMultiplayerServer
    {
        /// <summary>
        /// Request to join a multiplayer room.
        /// </summary>
        /// <param name="roomId">The databased room ID.</param>
        /// <returns>Whether the room could be joined.</returns>
        Task<bool> JoinRoom(long roomId);

        /// <summary>
        /// Request to leave the currently joined room.
        /// </summary>
        Task LeaveRoom();
    }
}
