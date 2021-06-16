// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// Interface for an out-of-room multiplayer server.
    /// </summary>
    public interface IMultiplayerLoungeServer
    {
        /// <summary>
        /// Request to join a multiplayer room.
        /// </summary>
        /// <param name="roomId">The databased room ID.</param>
        /// <exception cref="InvalidStateException">If the user is already in the requested (or another) room.</exception>
        Task<MultiplayerRoom> JoinRoom(long roomId);
    }
}
