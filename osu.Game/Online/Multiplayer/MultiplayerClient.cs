// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;

namespace osu.Game.Online.Multiplayer
{
    public class MultiplayerClient : StatefulMultiplayerClient
    {
        public override IBindable<bool> IsConnected => isConnected;

        private readonly Bindable<bool> isConnected = new Bindable<bool>();
        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        private readonly SemaphoreSlim connectionLock = new SemaphoreSlim(1);

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private HubConnection? connection;

        private CancellationTokenSource connectCancelSource = new CancellationTokenSource();

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
                    Task.Run(() => disconnect(true)).CatchUnobservedExceptions();
                    break;

                case APIState.Online:
                    Task.Run(connect).CatchUnobservedExceptions();
                    break;
            }
        }

        private async Task connect()
        {
            cancelExistingConnect();

            await connectionLock.WaitAsync(10000);

            try
            {
                await disconnect(false);

                // this token will be valid for the scope of this connection.
                // if cancelled, we can be sure that a disconnect or reconnect is handled elsewhere.
                var cancellationToken = connectCancelSource.Token;

                while (api.State.Value == APIState.Online)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Logger.Log("Multiplayer client connecting...", LoggingTarget.Network);

                    try
                    {
                        // importantly, rebuild the connection each attempt to get an updated access token.
                        connection = createConnection(cancellationToken);

                        await connection.StartAsync(cancellationToken);

                        Logger.Log("Multiplayer client connected!", LoggingTarget.Network);
                        isConnected.Value = true;
                        return;
                    }
                    catch (OperationCanceledException)
                    {
                        //connection process was cancelled.
                        throw;
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Multiplayer client connection error: {e}", LoggingTarget.Network);

                        // retry on any failure.
                        await Task.Delay(5000, cancellationToken);
                    }
                }
            }
            finally
            {
                connectionLock.Release();
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

        public override Task ChangeBeatmapAvailability(BeatmapAvailability newBeatmapAvailability)
        {
            if (!isConnected.Value)
                return Task.CompletedTask;

            return connection.InvokeAsync(nameof(IMultiplayerServer.ChangeBeatmapAvailability), newBeatmapAvailability);
        }

        public override Task StartMatch()
        {
            if (!isConnected.Value)
                return Task.CompletedTask;

            return connection.InvokeAsync(nameof(IMultiplayerServer.StartMatch));
        }

        private async Task disconnect(bool takeLock)
        {
            cancelExistingConnect();

            if (takeLock)
                await connectionLock.WaitAsync(10000);

            try
            {
                if (connection != null)
                    await connection.DisposeAsync();
            }
            finally
            {
                connection = null;
                if (takeLock)
                    connectionLock.Release();
            }
        }

        private void cancelExistingConnect()
        {
            connectCancelSource.Cancel();
            connectCancelSource = new CancellationTokenSource();
        }

        private HubConnection createConnection(CancellationToken cancellationToken)
        {
            var newConnection = new HubConnectionBuilder()
                                .WithUrl(endpoint, options => { options.Headers.Add("Authorization", $"Bearer {api.AccessToken}"); })
                                .AddNewtonsoftJsonProtocol(options => { options.PayloadSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; })
                                .Build();

            // this is kind of SILLY
            // https://github.com/dotnet/aspnetcore/issues/15198
            newConnection.On<MultiplayerRoomState>(nameof(IMultiplayerClient.RoomStateChanged), ((IMultiplayerClient)this).RoomStateChanged);
            newConnection.On<MultiplayerRoomUser>(nameof(IMultiplayerClient.UserJoined), ((IMultiplayerClient)this).UserJoined);
            newConnection.On<MultiplayerRoomUser>(nameof(IMultiplayerClient.UserLeft), ((IMultiplayerClient)this).UserLeft);
            newConnection.On<int>(nameof(IMultiplayerClient.HostChanged), ((IMultiplayerClient)this).HostChanged);
            newConnection.On<MultiplayerRoomSettings>(nameof(IMultiplayerClient.SettingsChanged), ((IMultiplayerClient)this).SettingsChanged);
            newConnection.On<int, MultiplayerUserState>(nameof(IMultiplayerClient.UserStateChanged), ((IMultiplayerClient)this).UserStateChanged);
            newConnection.On(nameof(IMultiplayerClient.LoadRequested), ((IMultiplayerClient)this).LoadRequested);
            newConnection.On(nameof(IMultiplayerClient.MatchStarted), ((IMultiplayerClient)this).MatchStarted);
            newConnection.On(nameof(IMultiplayerClient.ResultsReady), ((IMultiplayerClient)this).ResultsReady);

            newConnection.Closed += ex =>
            {
                isConnected.Value = false;

                Logger.Log(ex != null ? $"Multiplayer client lost connection: {ex}" : "Multiplayer client disconnected", LoggingTarget.Network);

                // make sure a disconnect wasn't triggered (and this is still the active connection).
                if (!cancellationToken.IsCancellationRequested)
                    Task.Run(connect, default).CatchUnobservedExceptions();

                return Task.CompletedTask;
            };
            return newConnection;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            cancelExistingConnect();
        }
    }
}
