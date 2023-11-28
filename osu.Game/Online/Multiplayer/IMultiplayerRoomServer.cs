// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;

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
        /// As the host, kick another user from the room.
        /// </summary>
        /// <param name="userId">The user to kick..</param>
        /// <exception cref="NotHostException">A user other than the current host is attempting to kick a user.</exception>
        /// <exception cref="NotJoinedRoomException">If the user is not in a room.</exception>
        Task KickUser(int userId);

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
        /// Change the local user's availability state of the current beatmap set in joined room.
        /// </summary>
        /// <param name="newBeatmapAvailability">The proposed new beatmap availability state.</param>
        Task ChangeBeatmapAvailability(BeatmapAvailability newBeatmapAvailability);

        /// <summary>
        /// Change the local user's mods in the currently joined room.
        /// </summary>
        /// <param name="newMods">The proposed new mods, excluding any required by the room itself.</param>
        Task ChangeUserMods(IEnumerable<APIMod> newMods);

        /// <summary>
        /// Send a match type specific request.
        /// </summary>
        /// <param name="request">The request to send.</param>
        Task SendMatchRequest(MatchUserRequest request);

        /// <summary>
        /// As the host of a room, start the match.
        /// </summary>
        /// <exception cref="NotHostException">A user other than the current host is attempting to start the game.</exception>
        /// <exception cref="NotJoinedRoomException">If the user is not in a room.</exception>
        /// <exception cref="InvalidStateException">If an attempt to start the game occurs when the game's (or users') state disallows it.</exception>
        Task StartMatch();

        /// <summary>
        /// Aborts an ongoing gameplay load.
        /// </summary>
        Task AbortGameplay();

        /// <summary>
        /// Adds an item to the playlist.
        /// </summary>
        /// <param name="item">The item to add.</param>
        Task AddPlaylistItem(MultiplayerPlaylistItem item);

        /// <summary>
        /// Edits an existing playlist item with new values.
        /// </summary>
        /// <param name="item">The item to edit, containing new properties. Must have an ID.</param>
        Task EditPlaylistItem(MultiplayerPlaylistItem item);

        /// <summary>
        /// Removes an item from the playlist.
        /// </summary>
        /// <param name="playlistItemId">The item to remove.</param>
        Task RemovePlaylistItem(long playlistItemId);

        /// <summary>
        /// Invites a player to the current room.
        /// </summary>
        /// <param name="userId">The user to invite.</param>
        /// <exception cref="UserBlockedException">The user has blocked or has been blocked by the invited user.</exception>
        /// <exception cref="UserBlocksPMsException">The invited user does not accept private messages.</exception>
        Task InvitePlayer(int userId);
    }
}
