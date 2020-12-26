// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game.Online.API;

namespace osu.Game.Online.Multiplayer
{
    public class MultiplayerClient : StatefulMultiplayerClient
    {
        public override IBindable<bool> IsConnected => isConnected;

        private readonly Bindable<bool> isConnected = new Bindable<bool>();
        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private HubConnection? connection;

        private readonly string endpoint;

        public MultiplayerClient(EndpointConfiguration endpoints)
        {
            endpoint = endpoints.MultiplayerEndpointUrl;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            apiState.BindTo(api.State);
            apiState.BindValueChanged(apiStateChanged, true);
        }

        private void apiStateChanged(ValueChangedEvent<APIState> state)
        {
            switch (state.NewValue)
            {
                case APIState.Failing:
                case APIState.Offline:
                    connection?.StopAsync();
                    connection = null;
                    break;

                case APIState.Online:
                    Task.Run(Connect);
                    break;
            }
        }

        protected virtual async Task Connect()
        {
            if (connection != null)
                return;

            connection = new HubConnectionBuilder()
                         .WithUrl(endpoint, options =>
                         {
                             options.Headers.Add("Authorization", $"Bearer {api.AccessToken}");
                         })
                         .AddNewtonsoftJsonProtocol(options => { options.PayloadSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; })
                         .Build();

            // this is kind of SILLY
            // https://github.com/dotnet/aspnetcore/issues/15198
            connection.On<MultiplayerRoomState>(nameof(IMultiplayerClient.RoomStateChanged), ((IMultiplayerClient)this).RoomStateChanged);
            connection.On<MultiplayerRoomUser>(nameof(IMultiplayerClient.UserJoined), ((IMultiplayerClient)this).UserJoined);
            connection.On<MultiplayerRoomUser>(nameof(IMultiplayerClient.UserLeft), ((IMultiplayerClient)this).UserLeft);
            connection.On<int>(nameof(IMultiplayerClient.HostChanged), ((IMultiplayerClient)this).HostChanged);
            connection.On<MultiplayerRoomSettings>(nameof(IMultiplayerClient.SettingsChanged), ((IMultiplayerClient)this).SettingsChanged);
            connection.On<int, MultiplayerUserState>(nameof(IMultiplayerClient.UserStateChanged), ((IMultiplayerClient)this).UserStateChanged);
            connection.On(nameof(IMultiplayerClient.LoadRequested), ((IMultiplayerClient)this).LoadRequested);
            connection.On(nameof(IMultiplayerClient.MatchStarted), ((IMultiplayerClient)this).MatchStarted);
            connection.On(nameof(IMultiplayerClient.ResultsReady), ((IMultiplayerClient)this).ResultsReady);

            connection.Closed += async ex =>
            {
                isConnected.Value = false;

                if (ex != null)
                {
                    Logger.Log($"Multiplayer client lost connection: {ex}", LoggingTarget.Network);
                    await tryUntilConnected();
                }
            };

            await tryUntilConnected();

            async Task tryUntilConnected()
            {
                Logger.Log("Multiplayer client connecting...", LoggingTarget.Network);

                while (api.State.Value == APIState.Online)
                {
                    try
                    {
                        Debug.Assert(connection != null);

                        // reconnect on any failure
                        await connection.StartAsync();
                        Logger.Log("Multiplayer client connected!", LoggingTarget.Network);

                        // Success.
                        isConnected.Value = true;
                        break;
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Multiplayer client connection error: {e}", LoggingTarget.Network);
                        await Task.Delay(5000);
                    }
                }
            }
        }

        protected override Task<MultiplayerRoom> JoinRoom(long roomId)
        {
            if (!isConnected.Value)
                return Task.FromCanceled<MultiplayerRoom>(new CancellationToken(true));

            return connection.InvokeAsync<MultiplayerRoom>(nameof(IMultiplayerServer.JoinRoom), roomId);
        }

        public override async Task LeaveRoom()
        {
            if (!isConnected.Value)
            {
                // even if not connected, make sure the local room state can be cleaned up.
                await base.LeaveRoom();
                return;
            }

            if (Room == null)
                return;

            await base.LeaveRoom();
            await connection.InvokeAsync(nameof(IMultiplayerServer.LeaveRoom));
        }

        public override Task TransferHost(int userId)
        {
            if (!isConnected.Value)
                return Task.CompletedTask;

            return connection.InvokeAsync(nameof(IMultiplayerServer.TransferHost), userId);
        }

        public override Task ChangeSettings(MultiplayerRoomSettings settings)
        {
            if (!isConnected.Value)
                return Task.CompletedTask;

            return connection.InvokeAsync(nameof(IMultiplayerServer.ChangeSettings), settings);
        }

        public override Task ChangeState(MultiplayerUserState newState)
        {
            if (!isConnected.Value)
                return Task.CompletedTask;

            return connection.InvokeAsync(nameof(IMultiplayerServer.ChangeState), newState);
        }

        public override Task StartMatch()
        {
            if (!isConnected.Value)
                return Task.CompletedTask;

            return connection.InvokeAsync(nameof(IMultiplayerServer.StartMatch));
        }
    }
}
