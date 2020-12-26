// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// An interface defining a multiplayer client instance.
    /// </summary>
    public interface IMultiplayerClient
    {
        /// <summary>
        /// Signals that the room has changed state.
        /// </summary>
        /// <param name="state">The state of the room.</param>
        Task RoomStateChanged(MultiplayerRoomState state);

        /// <summary>
        /// Signals that a user has joined the room.
        /// </summary>
        /// <param name="user">The user.</param>
        Task UserJoined(MultiplayerRoomUser user);

        /// <summary>
        /// Signals that a user has left the room.
        /// </summary>
        /// <param name="user">The user.</param>
        Task UserLeft(MultiplayerRoomUser user);

        /// <summary>
        /// Signal that the host of the room has changed.
        /// </summary>
        /// <param name="userId">The user ID of the new host.</param>
        Task HostChanged(int userId);

        /// <summary>
        /// Signals that the settings for this room have changed.
        /// </summary>
        /// <param name="newSettings">The updated room settings.</param>
        Task SettingsChanged(MultiplayerRoomSettings newSettings);

        /// <summary>
        /// Signals that a user in this room changed their state.
        /// </summary>
        /// <param name="userId">The ID of the user performing a state change.</param>
        /// <param name="state">The new state of the user.</param>
        Task UserStateChanged(int userId, MultiplayerUserState state);

        /// <summary>
        /// Signals that a match is to be started. This will *only* be sent to clients which are to begin loading at this point.
        /// </summary>
        Task LoadRequested();

        /// <summary>
        /// Signals that a match has started. All users in the <see cref="MultiplayerUserState.Loaded"/> state should begin gameplay as soon as possible.
        /// </summary>
        Task MatchStarted();

        /// <summary>
        /// Signals that the match has ended, all players have finished and results are ready to be displayed.
        /// </summary>
        Task ResultsReady();
    }
}
