// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// An interface defining a multiplayer client instance.
    /// </summary>
    public interface IMultiplayerClient : IStatefulUserHubClient
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
        /// Signals that a user has been kicked from the room.
        /// </summary>
        /// <remarks>
        /// This will also be sent to the user that was kicked.
        /// </remarks>
        /// <param name="user">The user.</param>
        Task UserKicked(MultiplayerRoomUser user);

        /// <summary>
        /// Signals that the local user has been invited into a multiplayer room.
        /// </summary>
        /// <param name="invitedBy">Id of user that invited the player.</param>
        /// <param name="roomID">Id of the room the user got invited to.</param>
        /// <param name="password">Password to join the room.</param>
        Task Invited(int invitedBy, long roomID, string password);

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
        /// Signals that the match type state has changed for a user in this room.
        /// </summary>
        /// <param name="userId">The ID of the user performing a state change.</param>
        /// <param name="state">The new state of the user.</param>
        Task MatchUserStateChanged(int userId, MatchUserState state);

        /// <summary>
        /// Signals that the match type state has changed for this room.
        /// </summary>
        /// <param name="state">The new state of the room.</param>
        Task MatchRoomStateChanged(MatchRoomState state);

        /// <summary>
        /// Send a match type specific request.
        /// </summary>
        /// <param name="e">The event to handle.</param>
        Task MatchEvent(MatchServerEvent e);

        /// <summary>
        /// Signals that a user in this room changed their beatmap availability state.
        /// </summary>
        /// <param name="userId">The ID of the user whose beatmap availability state has changed.</param>
        /// <param name="beatmapAvailability">The new beatmap availability state of the user.</param>
        Task UserBeatmapAvailabilityChanged(int userId, BeatmapAvailability beatmapAvailability);

        /// <summary>
        /// Signals that a user in this room changed their local mods.
        /// </summary>
        /// <param name="userId">The ID of the user whose mods have changed.</param>
        /// <param name="mods">The user's new local mods.</param>
        Task UserModsChanged(int userId, IEnumerable<APIMod> mods);

        /// <summary>
        /// Signals that the match is starting and the loading of gameplay should be started. This will *only* be sent to clients which are to begin loading at this point.
        /// </summary>
        Task LoadRequested();

        /// <summary>
        /// Signals that gameplay has started.
        /// All users in the <see cref="MultiplayerUserState.Loaded"/> or <see cref="MultiplayerUserState.ReadyForGameplay"/> states should begin gameplay as soon as possible.
        /// </summary>
        Task GameplayStarted();

        /// <summary>
        /// Signals that gameplay has been aborted.
        /// </summary>
        /// <param name="reason">The reason why gameplay was aborted.</param>
        Task GameplayAborted(GameplayAbortReason reason);

        /// <summary>
        /// Signals that the match has ended, all players have finished and results are ready to be displayed.
        /// </summary>
        Task ResultsReady();

        /// <summary>
        /// Signals that an item has been added to the playlist.
        /// </summary>
        /// <param name="item">The added item.</param>
        Task PlaylistItemAdded(MultiplayerPlaylistItem item);

        /// <summary>
        /// Signals that an item has been removed from the playlist.
        /// </summary>
        /// <param name="playlistItemId">The removed item.</param>
        Task PlaylistItemRemoved(long playlistItemId);

        /// <summary>
        /// Signals that an item has been changed in the playlist.
        /// </summary>
        /// <param name="item">The changed item.</param>
        Task PlaylistItemChanged(MultiplayerPlaylistItem item);
    }
}
