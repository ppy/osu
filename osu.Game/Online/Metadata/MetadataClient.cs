// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Users;

namespace osu.Game.Online.Metadata
{
    public abstract partial class MetadataClient : Component, IMetadataClient, IMetadataServer
    {
        public abstract IBindable<bool> IsConnected { get; }

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
        /// Whether the client is currently receiving user presence updates from the server.
        /// </summary>
        public abstract IBindable<bool> IsWatchingUserPresence { get; }

        /// <summary>
        /// Dictionary keyed by user ID containing all of the <see cref="UserPresence"/> information about currently online users received from the server.
        /// </summary>
        public abstract IBindableDictionary<int, UserPresence> UserStates { get; }

        /// <inheritdoc/>
        public abstract Task UpdateActivity(UserActivity? activity);

        /// <inheritdoc/>
        public abstract Task UpdateStatus(UserStatus? status);

        /// <inheritdoc/>
        public abstract Task BeginWatchingUserPresence();

        /// <inheritdoc/>
        public abstract Task EndWatchingUserPresence();

        /// <inheritdoc/>
        public abstract Task UserPresenceUpdated(int userId, UserPresence? presence);

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
