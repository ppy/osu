// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// Interface for an in-room multiplayer server.
    /// </summary>
    public interface IMultiplayerRoomServer
    {
        /// <summary>
        /// Request to leave the currently joined room.
        /// </summary>
        /// <exception cref="NotJoinedRoomException">If the user is not in a room.</exception>
        Task LeaveRoom();

        /// <summary>
        /// Transfer the host of the currently joined room to another user in the room.
        /// </summary>
        /// <param name="userId">The new user which is to become host.</param>
        /// <exception cref="NotHostException">A user other than the current host is attempting to transfer host.</exception>
        /// <exception cref="NotJoinedRoomException">If the user is not in a room.</exception>
        Task TransferHost(int userId);

        /// <summary>
        /// As the host, update the settings of the currently joined room.
        /// </summary>
        /// <param name="settings">The new settings to apply.</param>
        /// <exception cref="NotHostException">A user other than the current host is attempting to transfer host.</exception>
        /// <exception cref="NotJoinedRoomException">If the user is not in a room.</exception>
        Task ChangeSettings(MultiplayerRoomSettings settings);

        /// <summary>
        /// Change the local user state in the currently joined room.
        /// </summary>
        /// <param name="newState">The proposed new state.</param>
        /// <exception cref="InvalidStateChangeException">If the state change requested is not valid, given the previous state or room state.</exception>
        /// <exception cref="NotJoinedRoomException">If the user is not in a room.</exception>
        Task ChangeState(MultiplayerUserState newState);

        /// <summary>
        /// As the host of a room, start the match.
        /// </summary>
        /// <exception cref="NotHostException">A user other than the current host is attempting to start the game.</exception>
        /// <exception cref="NotJoinedRoomException">If the user is not in a room.</exception>
        /// <exception cref="InvalidStateException">If an attempt to start the game occurs when the game's (or users') state disallows it.</exception>
        Task StartMatch();
    }
}
