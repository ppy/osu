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
        public override IBindable<bool> IsConnected => new BindableBool(true);

        public override IBindable<bool> IsWatchingUserPresence => isWatchingUserPresence;
        private readonly BindableBool isWatchingUserPresence = new BindableBool();

        public override IBindableDictionary<int, UserPresence> UserStates => userStates;
        private readonly BindableDictionary<int, UserPresence> userStates = new BindableDictionary<int, UserPresence>();

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
    }
}
