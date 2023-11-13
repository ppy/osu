// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.Notifications;
using osu.Game.Localisation;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// A <see cref="MultiplayerClient"/> with online connectivity.
    /// </summary>
    public partial class OnlineMultiplayerClient : MultiplayerClient
    {
        private readonly string endpoint;

        private IHubClientConnector? connector;

        public override IBindable<bool> IsConnected { get; } = new BindableBool();

        private HubConnection? connection => connector?.CurrentConnection;

        public OnlineMultiplayerClient(EndpointConfiguration endpoints)
        {
            endpoint = endpoints.MultiplayerEndpointUrl;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            // Importantly, we are intentionally not using MessagePack here to correctly support derived class serialization.
            // More information on the limitations / reasoning can be found in osu-server-spectator's initialisation code.
            connector = api.GetHubConnector(nameof(OnlineMultiplayerClient), endpoint);

            if (connector != null)
            {
                connector.ConfigureConnection = connection =>
                {
                    // this is kind of SILLY
                    // https://github.com/dotnet/aspnetcore/issues/15198
                    connection.On<MultiplayerRoomState>(nameof(IMultiplayerClient.RoomStateChanged), ((IMultiplayerClient)this).RoomStateChanged);
                    connection.On<MultiplayerRoomUser>(nameof(IMultiplayerClient.UserJoined), ((IMultiplayerClient)this).UserJoined);
                    connection.On<MultiplayerRoomUser>(nameof(IMultiplayerClient.UserLeft), ((IMultiplayerClient)this).UserLeft);
                    connection.On<MultiplayerRoomUser>(nameof(IMultiplayerClient.UserKicked), ((IMultiplayerClient)this).UserKicked);
                    connection.On<int, long, string>(nameof(IMultiplayerClient.Invited), ((IMultiplayerClient)this).Invited);
                    connection.On<int>(nameof(IMultiplayerClient.HostChanged), ((IMultiplayerClient)this).HostChanged);
                    connection.On<MultiplayerRoomSettings>(nameof(IMultiplayerClient.SettingsChanged), ((IMultiplayerClient)this).SettingsChanged);
                    connection.On<int, MultiplayerUserState>(nameof(IMultiplayerClient.UserStateChanged), ((IMultiplayerClient)this).UserStateChanged);
                    connection.On(nameof(IMultiplayerClient.LoadRequested), ((IMultiplayerClient)this).LoadRequested);
                    connection.On(nameof(IMultiplayerClient.GameplayStarted), ((IMultiplayerClient)this).GameplayStarted);
                    connection.On(nameof(IMultiplayerClient.LoadAborted), ((IMultiplayerClient)this).LoadAborted);
                    connection.On(nameof(IMultiplayerClient.ResultsReady), ((IMultiplayerClient)this).ResultsReady);
                    connection.On<int, IEnumerable<APIMod>>(nameof(IMultiplayerClient.UserModsChanged), ((IMultiplayerClient)this).UserModsChanged);
                    connection.On<int, BeatmapAvailability>(nameof(IMultiplayerClient.UserBeatmapAvailabilityChanged), ((IMultiplayerClient)this).UserBeatmapAvailabilityChanged);
                    connection.On<MatchRoomState>(nameof(IMultiplayerClient.MatchRoomStateChanged), ((IMultiplayerClient)this).MatchRoomStateChanged);
                    connection.On<int, MatchUserState>(nameof(IMultiplayerClient.MatchUserStateChanged), ((IMultiplayerClient)this).MatchUserStateChanged);
                    connection.On<MatchServerEvent>(nameof(IMultiplayerClient.MatchEvent), ((IMultiplayerClient)this).MatchEvent);
                    connection.On<MultiplayerPlaylistItem>(nameof(IMultiplayerClient.PlaylistItemAdded), ((IMultiplayerClient)this).PlaylistItemAdded);
                    connection.On<long>(nameof(IMultiplayerClient.PlaylistItemRemoved), ((IMultiplayerClient)this).PlaylistItemRemoved);
                    connection.On<MultiplayerPlaylistItem>(nameof(IMultiplayerClient.PlaylistItemChanged), ((IMultiplayerClient)this).PlaylistItemChanged);
                    connection.On(nameof(IStatefulUserHubClient.DisconnectRequested), ((IMultiplayerClient)this).DisconnectRequested);
                };

                IsConnected.BindTo(connector.IsConnected);
            }
        }

        protected override async Task<MultiplayerRoom> JoinRoom(long roomId, string? password = null)
        {
            if (!IsConnected.Value)
                throw new OperationCanceledException();

            Debug.Assert(connection != null);

            try
            {
                return await connection.InvokeAsync<MultiplayerRoom>(nameof(IMultiplayerServer.JoinRoomWithPassword), roomId, password ?? string.Empty).ConfigureAwait(false);
            }
            catch (HubException exception)
            {
                if (exception.GetHubExceptionMessage() == HubClientConnector.SERVER_SHUTDOWN_MESSAGE)
                {
                    Debug.Assert(connector != null);

                    await connector.Reconnect().ConfigureAwait(false);
                    return await JoinRoom(roomId, password).ConfigureAwait(false);
                }

                throw;
            }
        }

        protected override Task LeaveRoomInternal()
        {
            if (!IsConnected.Value)
                return Task.FromCanceled(new CancellationToken(true));

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(IMultiplayerServer.LeaveRoom));
        }

        public override async Task InvitePlayer(int userId)
        {
            if (!IsConnected.Value)
                return;

            Debug.Assert(connection != null);

            try
            {
                await connection.InvokeAsync(nameof(IMultiplayerServer.InvitePlayer), userId).ConfigureAwait(false);
            }
            catch (HubException exception)
            {
                switch (exception.GetHubExceptionMessage())
                {
                    case UserBlockedException.MESSAGE:
                        PostNotification?.Invoke(new SimpleErrorNotification { Text = OnlinePlayStrings.InviteFailedUserBlocked });
                        break;

                    case UserBlocksPMsException.MESSAGE:
                        PostNotification?.Invoke(new SimpleErrorNotification { Text = OnlinePlayStrings.InviteFailedUserOptOut });
                        break;
                }
            }
        }

        public override Task TransferHost(int userId)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(IMultiplayerServer.TransferHost), userId);
        }

        public override Task KickUser(int userId)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(IMultiplayerServer.KickUser), userId);
        }

        public override Task ChangeSettings(MultiplayerRoomSettings settings)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(IMultiplayerServer.ChangeSettings), settings);
        }

        public override Task ChangeState(MultiplayerUserState newState)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(IMultiplayerServer.ChangeState), newState);
        }

        public override Task ChangeBeatmapAvailability(BeatmapAvailability newBeatmapAvailability)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(IMultiplayerServer.ChangeBeatmapAvailability), newBeatmapAvailability);
        }

        public override Task ChangeUserMods(IEnumerable<APIMod> newMods)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(IMultiplayerServer.ChangeUserMods), newMods);
        }

        public override Task SendMatchRequest(MatchUserRequest request)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(IMultiplayerServer.SendMatchRequest), request);
        }

        public override Task StartMatch()
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(IMultiplayerServer.StartMatch));
        }

        public override Task AbortGameplay()
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(IMultiplayerServer.AbortGameplay));
        }

        public override Task AddPlaylistItem(MultiplayerPlaylistItem item)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(IMultiplayerServer.AddPlaylistItem), item);
        }

        public override Task EditPlaylistItem(MultiplayerPlaylistItem item)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(IMultiplayerServer.EditPlaylistItem), item);
        }

        public override Task RemovePlaylistItem(long playlistItemId)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(IMultiplayerServer.RemovePlaylistItem), playlistItemId);
        }

        public override Task DisconnectInternal()
        {
            if (connector == null)
                return Task.CompletedTask;

            return connector.Disconnect();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            connector?.Dispose();
        }
    }
}
