// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.API;
using osu.Game.Online.Metadata;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Metadata
{
    public partial class TestMetadataClient : MetadataClient
    {
        public override IBindable<bool> IsConnected => isConnected;
        private readonly BindableBool isConnected = new BindableBool(true);

        public override UserPresence LocalUserPresence => localUserPresence;
        private UserPresence localUserPresence;

        public override IBindableDictionary<int, UserPresence> UserPresences => userPresences;
        private readonly BindableDictionary<int, UserPresence> userPresences = new BindableDictionary<int, UserPresence>();

        public override IBindableDictionary<int, UserPresence> FriendPresences => friendPresences;
        private readonly BindableDictionary<int, UserPresence> friendPresences = new BindableDictionary<int, UserPresence>();

        public override Bindable<DailyChallengeInfo?> DailyChallengeInfo => dailyChallengeInfo;
        private readonly Bindable<DailyChallengeInfo?> dailyChallengeInfo = new Bindable<DailyChallengeInfo?>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        public event Action? OnBeginWatchingUserPresence;
        public event Action? OnEndWatchingUserPresence;

        protected override Task BeginWatchingUserPresenceInternal()
        {
            OnBeginWatchingUserPresence?.Invoke();
            return Task.CompletedTask;
        }

        protected override Task EndWatchingUserPresenceInternal()
        {
            OnEndWatchingUserPresence?.Invoke();
            return Task.CompletedTask;
        }

        public override Task UpdateActivity(UserActivity? activity)
        {
            localUserPresence = localUserPresence with { Activity = activity };

            if (IsWatchingUserPresence)
            {
                if (userPresences.ContainsKey(api.LocalUser.Value.Id))
                    userPresences[api.LocalUser.Value.Id] = localUserPresence;
            }

            return Task.CompletedTask;
        }

        public override Task UpdateStatus(UserStatus? status)
        {
            localUserPresence = localUserPresence with { Status = status };

            if (IsWatchingUserPresence)
            {
                if (userPresences.ContainsKey(api.LocalUser.Value.Id))
                    userPresences[api.LocalUser.Value.Id] = localUserPresence;
            }

            return Task.CompletedTask;
        }

        public override Task UserPresenceUpdated(int userId, UserPresence? presence)
        {
            if (IsWatchingUserPresence)
            {
                if (presence?.Status != null)
                {
                    if (userId == api.LocalUser.Value.OnlineID)
                        localUserPresence = presence.Value;
                    else
                        userPresences[userId] = presence.Value;
                }
                else
                {
                    if (userId == api.LocalUser.Value.OnlineID)
                        localUserPresence = default;
                    else
                        userPresences.Remove(userId);
                }
            }

            return Task.CompletedTask;
        }

        public override Task FriendPresenceUpdated(int userId, UserPresence? presence)
        {
            if (presence.HasValue)
                friendPresences[userId] = presence.Value;
            else
                friendPresences.Remove(userId);

            return Task.CompletedTask;
        }

        public override Task<BeatmapUpdates> GetChangesSince(int queueId)
            => Task.FromResult(new BeatmapUpdates(Array.Empty<int>(), queueId));

        public override Task BeatmapSetsUpdated(BeatmapUpdates updates) => Task.CompletedTask;

        public override Task DailyChallengeUpdated(DailyChallengeInfo? info)
        {
            dailyChallengeInfo.Value = info;
            return Task.CompletedTask;
        }

        public override Task<MultiplayerPlaylistItemStats[]> BeginWatchingMultiplayerRoom(long id)
        {
            var stats = new MultiplayerPlaylistItemStats[MultiplayerPlaylistItemStats.TOTAL_SCORE_DISTRIBUTION_BINS];

            for (int i = 0; i < stats.Length; i++)
                stats[i] = new MultiplayerPlaylistItemStats { PlaylistItemID = i };

            return Task.FromResult(stats);
        }

        public override Task EndWatchingMultiplayerRoom(long id) => Task.CompletedTask;

        public void Disconnect()
        {
            isConnected.Value = false;
            dailyChallengeInfo.Value = null;
        }

        public void Reconnect()
        {
            isConnected.Value = true;
        }
    }
}
