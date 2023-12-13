// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Game.Users;

namespace osu.Game.Online.Metadata
{
    /// <summary>
    /// Metadata server is responsible for keeping the osu! client up-to-date with any changes.
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
    }
}
