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

        public override IBindable<bool> IsWatchingUserPresence => isWatchingUserPresence;
        private readonly BindableBool isWatchingUserPresence = new BindableBool();

        public override IBindableDictionary<int, UserPresence> UserStates => userStates;
        private readonly BindableDictionary<int, UserPresence> userStates = new BindableDictionary<int, UserPresence>();

        public override Bindable<DailyChallengeInfo?> DailyChallengeInfo => dailyChallengeInfo;
        private readonly Bindable<DailyChallengeInfo?> dailyChallengeInfo = new Bindable<DailyChallengeInfo?>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        public override Task BeginWatchingUserPresence()
        {
            isWatchingUserPresence.Value = true;
            return Task.CompletedTask;
        }

        public override Task EndWatchingUserPresence()
        {
            isWatchingUserPresence.Value = false;
            return Task.CompletedTask;
        }

        public override Task UpdateActivity(UserActivity? activity)
        {
            if (isWatchingUserPresence.Value)
            {
                userStates.TryGetValue(api.LocalUser.Value.Id, out var localUserPresence);
                localUserPresence = localUserPresence with { Activity = activity };
                userStates[api.LocalUser.Value.Id] = localUserPresence;
            }

            return Task.CompletedTask;
        }

        public override Task UpdateStatus(UserStatus? status)
        {
            if (isWatchingUserPresence.Value)
            {
                userStates.TryGetValue(api.LocalUser.Value.Id, out var localUserPresence);
                localUserPresence = localUserPresence with { Status = status };
                userStates[api.LocalUser.Value.Id] = localUserPresence;
            }

            return Task.CompletedTask;
        }

        public override Task UserPresenceUpdated(int userId, UserPresence? presence)
        {
            if (isWatchingUserPresence.Value)
            {
                if (presence.HasValue)
                    userStates[userId] = presence.Value;
                else
                    userStates.Remove(userId);
            }

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
