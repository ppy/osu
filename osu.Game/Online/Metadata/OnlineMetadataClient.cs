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
using osu.Game.Users;

namespace osu.Game.Online.Metadata
{
    public partial class OnlineMetadataClient : MetadataClient
    {
        public override IBindable<bool> IsConnected { get; } = new Bindable<bool>();

        public override IBindable<bool> IsWatchingUserPresence => isWatchingUserPresence;
        private readonly BindableBool isWatchingUserPresence = new BindableBool();

        public override IBindableDictionary<int, UserPresence> UserStates => userStates;
        private readonly BindableDictionary<int, UserPresence> userStates = new BindableDictionary<int, UserPresence>();

        private readonly string endpoint;

        private IHubClientConnector? connector;

        private Bindable<int> lastQueueId = null!;

        private IBindable<APIUser> localUser = null!;
        private IBindable<UserActivity?> userActivity = null!;
        private IBindable<UserStatus?>? userStatus;

        private HubConnection? connection => connector?.CurrentConnection;

        public OnlineMetadataClient(EndpointConfiguration endpoints)
        {
            endpoint = endpoints.MetadataEndpointUrl;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, OsuConfigManager config)
        {
            // Importantly, we are intentionally not using MessagePack here to correctly support derived class serialization.
            // More information on the limitations / reasoning can be found in osu-server-spectator's initialisation code.
            connector = api.GetHubConnector(nameof(OnlineMetadataClient), endpoint, false);

            if (connector != null)
            {
                connector.ConfigureConnection = connection =>
                {
                    // this is kind of SILLY
                    // https://github.com/dotnet/aspnetcore/issues/15198
                    connection.On<BeatmapUpdates>(nameof(IMetadataClient.BeatmapSetsUpdated), ((IMetadataClient)this).BeatmapSetsUpdated);
                    connection.On<int, UserPresence?>(nameof(IMetadataClient.UserPresenceUpdated), ((IMetadataClient)this).UserPresenceUpdated);
                    connection.On(nameof(IStatefulUserHubClient.DisconnectRequested), ((IMetadataClient)this).DisconnectRequested);
                };

                IsConnected.BindTo(connector.IsConnected);
                IsConnected.BindValueChanged(isConnectedChanged, true);
            }

            lastQueueId = config.GetBindable<int>(OsuSetting.LastProcessedMetadataId);

            localUser = api.LocalUser.GetBoundCopy();
            userActivity = api.Activity.GetBoundCopy()!;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            localUser.BindValueChanged(_ =>
            {
                if (localUser.Value is not GuestUser)
                {
                    userStatus = localUser.Value.Status.GetBoundCopy();
                    userStatus.BindValueChanged(status => UpdateStatus(status.NewValue), true);
                }
                else
                    userStatus = null;
            }, true);
            userActivity.BindValueChanged(activity =>
            {
                if (localUser.Value is not GuestUser)
                    UpdateActivity(activity.NewValue);
            }, true);
        }

        private bool catchingUp;

        private void isConnectedChanged(ValueChangedEvent<bool> connected)
        {
            if (!connected.NewValue)
            {
                Schedule(() =>
                {
                    isWatchingUserPresence.Value = false;
                    userStates.Clear();
                });
                return;
            }

            if (localUser.Value is not GuestUser)
            {
                UpdateActivity(userActivity.Value);
                UpdateStatus(userStatus?.Value);
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

        public override Task UserPresenceUpdated(int userId, UserPresence? presence)
        {
            Schedule(() =>
            {
                if (presence?.Status != null)
                    userStates[userId] = presence.Value;
                else
                    userStates.Remove(userId);
            });

            return Task.CompletedTask;
        }

        public override async Task BeginWatchingUserPresence()
        {
            if (connector?.IsConnected.Value != true)
                throw new OperationCanceledException();

            Debug.Assert(connection != null);
            await connection.InvokeAsync(nameof(IMetadataServer.BeginWatchingUserPresence)).ConfigureAwait(false);
            Schedule(() => isWatchingUserPresence.Value = true);
        }

        public override async Task EndWatchingUserPresence()
        {
            try
            {
                if (connector?.IsConnected.Value != true)
                    throw new OperationCanceledException();

                // must be scheduled before any remote calls to avoid mis-ordering.
                Schedule(() => userStates.Clear());
                Debug.Assert(connection != null);
                await connection.InvokeAsync(nameof(IMetadataServer.EndWatchingUserPresence)).ConfigureAwait(false);
            }
            finally
            {
                Schedule(() => isWatchingUserPresence.Value = false);
            }
        }

        public override async Task DisconnectRequested()
        {
            await base.DisconnectRequested().ConfigureAwait(false);
            await EndWatchingUserPresence().ConfigureAwait(false);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            connector?.Dispose();
        }
    }
}
