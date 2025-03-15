// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Users;

namespace osu.Game.Online.Metadata
{
    public abstract partial class MetadataClient : Component, IMetadataClient, IMetadataServer
    {
        public abstract IBindable<bool> IsConnected { get; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        #region Beatmap metadata updates

        public abstract Task<BeatmapUpdates> GetChangesSince(int queueId);

        public abstract Task BeatmapSetsUpdated(BeatmapUpdates updates);

        public event Action<int[]>? ChangedBeatmapSetsArrived;

        protected Task ProcessChanges(int[] beatmapSetIDs)
        {
            ChangedBeatmapSetsArrived?.Invoke(beatmapSetIDs.Distinct().ToArray());
            return Task.CompletedTask;
        }

        #endregion

        #region User presence updates

        /// <summary>
        /// The <see cref="UserPresence"/> information about the current user.
        /// </summary>
        public abstract UserPresence LocalUserPresence { get; }

        /// <summary>
        /// Dictionary keyed by user ID containing all of the <see cref="UserPresence"/> information about currently online users received from the server.
        /// </summary>
        public abstract IBindableDictionary<int, UserPresence> UserPresences { get; }

        /// <summary>
        /// Dictionary keyed by user ID containing all of the <see cref="UserPresence"/> information about currently online friends received from the server.
        /// </summary>
        public abstract IBindableDictionary<int, UserPresence> FriendPresences { get; }

        /// <summary>
        /// Attempts to retrieve the presence of a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The user presence, or null if not available or the user's offline.</returns>
        public UserPresence? GetPresence(int userId)
        {
            if (userId == api.LocalUser.Value.OnlineID)
                return LocalUserPresence;

            if (FriendPresences.TryGetValue(userId, out UserPresence presence))
                return presence;

            if (UserPresences.TryGetValue(userId, out presence))
                return presence;

            return null;
        }

        public abstract Task UpdateActivity(UserActivity? activity);

        public abstract Task UpdateStatus(UserStatus? status);

        private int userPresenceWatchCount;

        protected bool IsWatchingUserPresence
            => Interlocked.CompareExchange(ref userPresenceWatchCount, userPresenceWatchCount, userPresenceWatchCount) > 0;

        /// <summary>
        /// Signals to the server that we want to begin receiving status updates for all users.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> which will end the session when disposed.</returns>
        public IDisposable BeginWatchingUserPresence() => new UserPresenceWatchToken(this);

        Task IMetadataServer.BeginWatchingUserPresence()
        {
            if (Interlocked.Increment(ref userPresenceWatchCount) == 1)
                return BeginWatchingUserPresenceInternal();

            return Task.CompletedTask;
        }

        Task IMetadataServer.EndWatchingUserPresence()
        {
            if (Interlocked.Decrement(ref userPresenceWatchCount) == 0)
                return EndWatchingUserPresenceInternal();

            return Task.CompletedTask;
        }

        protected abstract Task BeginWatchingUserPresenceInternal();

        protected abstract Task EndWatchingUserPresenceInternal();

        public abstract Task UserPresenceUpdated(int userId, UserPresence? presence);

        public abstract Task FriendPresenceUpdated(int userId, UserPresence? presence);

        private class UserPresenceWatchToken : IDisposable
        {
            private readonly IMetadataServer server;
            private bool isDisposed;

            public UserPresenceWatchToken(IMetadataServer server)
            {
                this.server = server;
                server.BeginWatchingUserPresence().FireAndForget();
            }

            public void Dispose()
            {
                if (isDisposed)
                    return;

                server.EndWatchingUserPresence().FireAndForget();
                isDisposed = true;
            }
        }

        #endregion

        #region Daily Challenge

        public abstract IBindable<DailyChallengeInfo?> DailyChallengeInfo { get; }

        public abstract Task DailyChallengeUpdated(DailyChallengeInfo? info);

        #endregion

        #region Multiplayer room watching

        public abstract Task<MultiplayerPlaylistItemStats[]> BeginWatchingMultiplayerRoom(long id);

        public abstract Task EndWatchingMultiplayerRoom(long id);

        public event Action<MultiplayerRoomScoreSetEvent>? MultiplayerRoomScoreSet;

        Task IMetadataClient.MultiplayerRoomScoreSet(MultiplayerRoomScoreSetEvent roomScoreSetEvent)
        {
            if (MultiplayerRoomScoreSet != null)
                Schedule(MultiplayerRoomScoreSet, roomScoreSetEvent);

            return Task.CompletedTask;
        }

        #endregion

        #region Disconnection handling

        public event Action? Disconnecting;

        public virtual Task DisconnectRequested()
        {
            Schedule(() => Disconnecting?.Invoke());
            return Task.CompletedTask;
        }

        #endregion
    }
}
