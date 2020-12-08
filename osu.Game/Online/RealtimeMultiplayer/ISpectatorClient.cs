// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;

namespace osu.Game.Online.RealtimeMultiplayer
{
    /// <summary>
    /// An interface defining a spectator client instance.
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
        Task HostChanged(long userId);

        /// <summary>
        /// Signals that the settings for this room have changed.
        /// </summary>
        /// <param name="newSettings">The updated room settings.</param>
        Task SettingsChanged(MultiplayerRoomSettings newSettings);
    }
}
