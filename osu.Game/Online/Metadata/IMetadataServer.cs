// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Game.Users;

namespace osu.Game.Online.Metadata
{
    /// <summary>
    /// Metadata server is responsible for keeping the osu! client up-to-date with various real-time happenings, such as:
    /// <list type="bullet">
    /// <item>beatmap updates via BSS,</item>
    /// <item>online user activity/status updates,</item>
    /// <item>other real-time happenings, such as current "daily challenge" status.</item>
    /// </list>
    /// </summary>
    public interface IMetadataServer
    {
        /// <summary>
        /// Get any changes since a specific point in the queue.
        /// Should be used to allow the client to catch up with any changes after being closed or disconnected.
        /// </summary>
        /// <param name="queueId">The last processed queue ID.</param>
        /// <returns></returns>
        Task<BeatmapUpdates> GetChangesSince(int queueId);

        /// <summary>
        /// Signals to the server that the current user's <see cref="UserActivity"/> has changed.
        /// </summary>
        Task UpdateActivity(UserActivity? activity);

        /// <summary>
        /// Signals to the server that the current user's <see cref="UserStatus"/> has changed.
        /// </summary>
        Task UpdateStatus(UserStatus? status);

        /// <summary>
        /// Signals to the server that the current user would like to begin receiving updates on other users' online presence.
        /// </summary>
        Task BeginWatchingUserPresence();

        /// <summary>
        /// Signals to the server that the current user would like to stop receiving updates on other users' online presence.
        /// </summary>
        Task EndWatchingUserPresence();

        /// <summary>
        /// Signals to the server that the current user would like to begin receiving updates about the state of the multiplayer room with the given <paramref name="id"/>.
        /// </summary>
        Task<MultiplayerPlaylistItemStats[]> BeginWatchingMultiplayerRoom(long id);

        /// <summary>
        /// Signals to the server that the current user would like to stop receiving updates about the state of the multiplayer room with the given <paramref name="id"/>.
        /// </summary>
        Task EndWatchingMultiplayerRoom(long id);
    }
}
