// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Game.Users;

namespace osu.Game.Online.Metadata
{
    /// <summary>
    /// Interface for metadata-related remote procedure calls to be executed on the client side.
    /// </summary>
    public interface IMetadataClient : IStatefulUserHubClient
    {
        /// <summary>
        /// Delivers the set of requested <see cref="BeatmapUpdates"/> to the client.
        /// </summary>
        Task BeatmapSetsUpdated(BeatmapUpdates updates);

        /// <summary>
        /// Delivers an update of the <see cref="UserPresence"/> of the user with the supplied <paramref name="userId"/>.
        /// </summary>
        Task UserPresenceUpdated(int userId, UserPresence? status);

        /// <summary>
        /// Delivers and update of the <see cref="UserPresence"/> of a friend with the supplied <paramref name="userId"/>.
        /// </summary>
        Task FriendPresenceUpdated(int userId, UserPresence? presence);

        /// <summary>
        /// Delivers an update of the current "daily challenge" status.
        /// Null value means there is no "daily challenge" currently active.
        /// </summary>
        Task DailyChallengeUpdated(DailyChallengeInfo? info);

        /// <summary>
        /// Delivers information that a multiplayer score was set in a watched room.
        /// To receive these, the client must call <see cref="IMetadataServer.BeginWatchingMultiplayerRoom"/> for a given room first.
        /// </summary>
        Task MultiplayerRoomScoreSet(MultiplayerRoomScoreSetEvent roomScoreSetEvent);
    }
}
