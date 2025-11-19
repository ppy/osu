// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Users;

namespace osu.Game.Online.Metadata
{
    public partial class OnlineMetadataClient : MetadataClient
    {
        public override IBindable<bool> IsConnected { get; } = new Bindable<bool>();

        public override UserPresence LocalUserPresence => localUserPresence;
        private UserPresence localUserPresence;

        public override IBindableDictionary<int, UserPresence> UserPresences => userPresences;
        private readonly BindableDictionary<int, UserPresence> userPresences = new BindableDictionary<int, UserPresence>();

        public override IBindableDictionary<int, UserPresence> FriendPresences => friendPresences;
        private readonly BindableDictionary<int, UserPresence> friendPresences = new BindableDictionary<int, UserPresence>();

        public override IBindable<DailyChallengeInfo?> DailyChallengeInfo => dailyChallengeInfo;
        private readonly Bindable<DailyChallengeInfo?> dailyChallengeInfo = new Bindable<DailyChallengeInfo?>();

        private readonly string endpoint;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private IHubClientConnector? connector;
        private Bindable<int> lastQueueId = null!;
        private IBindable<APIUser> localUser = null!;
        private IBindable<UserStatus> userStatus = null!;
        private IBindable<UserActivity?> userActivity = null!;

        private HubConnection? connection => connector?.CurrentConnection;

        public OnlineMetadataClient(EndpointConfiguration endpoints)
        {
            endpoint = endpoints.MetadataUrl;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, SessionStatics session)
        {
            // Importantly, we are intentionally not using MessagePack here to correctly support derived class serialization.
            // More information on the limitations / reasoning can be found in osu-server-spectator's initialisation code.
            connector = api.GetHubConnector(nameof(OnlineMetadataClient), endpoint);

            if (connector != null)
            {
                connector.ConfigureConnection = connection =>
                {
                    // this is kind of SILLY
                    // https://github.com/dotnet/aspnetcore/issues/15198
                    connection.On<BeatmapUpdates>(nameof(IMetadataClient.BeatmapSetsUpdated), ((IMetadataClient)this).BeatmapSetsUpdated);
                    connection.On<int, UserPresence?>(nameof(IMetadataClient.UserPresenceUpdated), ((IMetadataClient)this).UserPresenceUpdated);
                    connection.On<int, UserPresence?>(nameof(IMetadataClient.FriendPresenceUpdated), ((IMetadataClient)this).FriendPresenceUpdated);
                    connection.On<DailyChallengeInfo?>(nameof(IMetadataClient.DailyChallengeUpdated), ((IMetadataClient)this).DailyChallengeUpdated);
                    connection.On<MultiplayerRoomScoreSetEvent>(nameof(IMetadataClient.MultiplayerRoomScoreSet), ((IMetadataClient)this).MultiplayerRoomScoreSet);
                    connection.On(nameof(IStatefulUserHubClient.DisconnectRequested), ((IMetadataClient)this).DisconnectRequested);
                };

                IsConnected.BindTo(connector.IsConnected);
                IsConnected.BindValueChanged(isConnectedChanged, true);
            }

            localUser = api.LocalUser.GetBoundCopy();
            lastQueueId = config.GetBindable<int>(OsuSetting.LastProcessedMetadataId);
            userStatus = config.GetBindable<UserStatus>(OsuSetting.UserOnlineStatus);
            userActivity = session.GetBindable<UserActivity?>(Static.UserOnlineActivity);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userStatus.BindValueChanged(status =>
            {
                if (localUser.Value is not GuestUser)
                    UpdateStatus(status.NewValue).FireAndForget();
            }, true);

            userActivity.BindValueChanged(activity =>
            {
                if (localUser.Value is not GuestUser)
                    UpdateActivity(activity.NewValue).FireAndForget();
            }, true);
        }

        private bool catchingUp;

        private void isConnectedChanged(ValueChangedEvent<bool> connected)
        {
            if (!connected.NewValue)
            {
                Schedule(() =>
                {
                    userPresences.Clear();
                    friendPresences.Clear();
                    dailyChallengeInfo.Value = null;
                    localUserPresence = default;
                });

                return;
            }

            if (IsWatchingUserPresence)
                BeginWatchingUserPresenceInternal().FireAndForget();

            if (localUser.Value is not GuestUser)
            {
                UpdateActivity(userActivity.Value).FireAndForget();
                UpdateStatus(userStatus.Value).FireAndForget();
            }

            if (lastQueueId.Value >= 0)
            {
                catchingUp = true;

                Task.Run(async () =>
                {
                    try
                    {
                        while (true)
                        {
                            Logger.Log($"Requesting catch-up from {lastQueueId.Value}");
                            var catchUpChanges = await GetChangesSince(lastQueueId.Value).ConfigureAwait(true);

                            lastQueueId.Value = catchUpChanges.LastProcessedQueueID;

                            if (catchUpChanges.BeatmapSetIDs.Length == 0)
                            {
                                Logger.Log($"Catch-up complete at {lastQueueId.Value}");
                                break;
                            }

                            await ProcessChanges(catchUpChanges.BeatmapSetIDs).ConfigureAwait(true);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Error while processing catch-up of metadata ({e.Message})");
                    }
                    finally
                    {
                        catchingUp = false;
                    }
                });
            }
        }

        public override async Task BeatmapSetsUpdated(BeatmapUpdates updates)
        {
            Logger.Log($"Received beatmap updates {updates.BeatmapSetIDs.Length} updates with last id {updates.LastProcessedQueueID}");

            // If we're still catching up, avoid updating the last ID as it will interfere with catch-up efforts.
            if (!catchingUp)
                lastQueueId.Value = updates.LastProcessedQueueID;

            await ProcessChanges(updates.BeatmapSetIDs).ConfigureAwait(false);
        }

        public override Task<BeatmapUpdates> GetChangesSince(int queueId)
        {
            if (connector?.IsConnected.Value != true)
                return Task.FromCanceled<BeatmapUpdates>(default);

            Logger.Log($"Requesting any changes since last known queue id {queueId}");

            Debug.Assert(connection != null);

            return connection.InvokeAsync<BeatmapUpdates>(nameof(IMetadataServer.GetChangesSince), queueId);
        }

        public override Task UpdateActivity(UserActivity? activity)
        {
            if (connector?.IsConnected.Value != true)
                return Task.FromCanceled(new CancellationToken(true));

            Debug.Assert(connection != null);
            return connection.InvokeAsync(nameof(IMetadataServer.UpdateActivity), activity);
        }

        public override Task UpdateStatus(UserStatus? status)
        {
            if (connector?.IsConnected.Value != true)
                return Task.FromCanceled(new CancellationToken(true));

            Debug.Assert(connection != null);
            return connection.InvokeAsync(nameof(IMetadataServer.UpdateStatus), status);
        }

        protected override Task BeginWatchingUserPresenceInternal()
        {
            if (connector?.IsConnected.Value != true)
                return Task.CompletedTask;

            Logger.Log($@"{nameof(OnlineMetadataClient)} began watching user presence", LoggingTarget.Network);

            Debug.Assert(connection != null);
            return connection.InvokeAsync(nameof(IMetadataServer.BeginWatchingUserPresence));
        }

        protected override Task EndWatchingUserPresenceInternal()
        {
            if (connector?.IsConnected.Value != true)
                return Task.CompletedTask;

            Logger.Log($@"{nameof(OnlineMetadataClient)} stopped watching user presence", LoggingTarget.Network);

            // must be scheduled before any remote calls to avoid mis-ordering.
            Schedule(() => userPresences.Clear());

            Debug.Assert(connection != null);
            return connection.InvokeAsync(nameof(IMetadataServer.EndWatchingUserPresence));
        }

        public override Task UserPresenceUpdated(int userId, UserPresence? presence)
        {
            Schedule(() =>
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
            });

            return Task.CompletedTask;
        }

        public override Task FriendPresenceUpdated(int userId, UserPresence? presence)
        {
            Schedule(() =>
            {
                if (presence?.Status != null)
                    friendPresences[userId] = presence.Value;
                else
                    friendPresences.Remove(userId);
            });

            return Task.CompletedTask;
        }

        public override Task DailyChallengeUpdated(DailyChallengeInfo? info)
        {
            Schedule(() => dailyChallengeInfo.Value = info);
            return Task.CompletedTask;
        }

        public override async Task<MultiplayerPlaylistItemStats[]> BeginWatchingMultiplayerRoom(long id)
        {
            if (connector?.IsConnected.Value != true)
                throw new OperationCanceledException();

            Debug.Assert(connection != null);
            var result = await connection.InvokeAsync<MultiplayerPlaylistItemStats[]>(nameof(IMetadataServer.BeginWatchingMultiplayerRoom), id).ConfigureAwait(false);
            Logger.Log($@"{nameof(OnlineMetadataClient)} began watching multiplayer room with ID {id}", LoggingTarget.Network);
            return result;
        }

        public override async Task EndWatchingMultiplayerRoom(long id)
        {
            if (connector?.IsConnected.Value != true)
                throw new OperationCanceledException();

            Debug.Assert(connection != null);
            await connection.InvokeAsync(nameof(IMetadataServer.EndWatchingMultiplayerRoom), id).ConfigureAwait(false);
            Logger.Log($@"{nameof(OnlineMetadataClient)} stopped watching multiplayer room with ID {id}", LoggingTarget.Network);
        }

        public override async Task DisconnectRequested()
        {
            await base.DisconnectRequested().ConfigureAwait(false);
            if (connector != null)
                await connector.Disconnect().ConfigureAwait(false);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            connector?.Dispose();
        }
    }
}
