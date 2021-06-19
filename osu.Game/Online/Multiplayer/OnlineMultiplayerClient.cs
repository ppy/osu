// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Rooms;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// A <see cref="MultiplayerClient"/> with online connectivity.
    /// </summary>
    public class OnlineMultiplayerClient : MultiplayerClient
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
                    connection.On<int>(nameof(IMultiplayerClient.HostChanged), ((IMultiplayerClient)this).HostChanged);
                    connection.On<MultiplayerRoomSettings>(nameof(IMultiplayerClient.SettingsChanged), ((IMultiplayerClient)this).SettingsChanged);
                    connection.On<int, MultiplayerUserState>(nameof(IMultiplayerClient.UserStateChanged), ((IMultiplayerClient)this).UserStateChanged);
                    connection.On(nameof(IMultiplayerClient.LoadRequested), ((IMultiplayerClient)this).LoadRequested);
                    connection.On(nameof(IMultiplayerClient.MatchStarted), ((IMultiplayerClient)this).MatchStarted);
                    connection.On(nameof(IMultiplayerClient.ResultsReady), ((IMultiplayerClient)this).ResultsReady);
                    connection.On<int, IEnumerable<APIMod>>(nameof(IMultiplayerClient.UserModsChanged), ((IMultiplayerClient)this).UserModsChanged);
                    connection.On<int, BeatmapAvailability>(nameof(IMultiplayerClient.UserBeatmapAvailabilityChanged), ((IMultiplayerClient)this).UserBeatmapAvailabilityChanged);
                };

                IsConnected.BindTo(connector.IsConnected);
            }
        }

        protected override Task<MultiplayerRoom> JoinRoom(long roomId)
        {
            if (!IsConnected.Value)
                return Task.FromCanceled<MultiplayerRoom>(new CancellationToken(true));

            return connection.InvokeAsync<MultiplayerRoom>(nameof(IMultiplayerServer.JoinRoom), roomId);
        }

        protected override Task LeaveRoomInternal()
        {
            if (!IsConnected.Value)
                return Task.FromCanceled(new CancellationToken(true));

            return connection.InvokeAsync(nameof(IMultiplayerServer.LeaveRoom));
        }

        public override Task TransferHost(int userId)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            return connection.InvokeAsync(nameof(IMultiplayerServer.TransferHost), userId);
        }

        public override Task ChangeSettings(MultiplayerRoomSettings settings)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            return connection.InvokeAsync(nameof(IMultiplayerServer.ChangeSettings), settings);
        }

        public override Task ChangeState(MultiplayerUserState newState)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            return connection.InvokeAsync(nameof(IMultiplayerServer.ChangeState), newState);
        }

        public override Task ChangeBeatmapAvailability(BeatmapAvailability newBeatmapAvailability)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            return connection.InvokeAsync(nameof(IMultiplayerServer.ChangeBeatmapAvailability), newBeatmapAvailability);
        }

        public override Task ChangeUserMods(IEnumerable<APIMod> newMods)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            return connection.InvokeAsync(nameof(IMultiplayerServer.ChangeUserMods), newMods);
        }

        public override Task StartMatch()
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            return connection.InvokeAsync(nameof(IMultiplayerServer.StartMatch));
        }

        protected override Task<BeatmapSetInfo> GetOnlineBeatmapSet(int beatmapId, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<BeatmapSetInfo>();
            var req = new GetBeatmapSetRequest(beatmapId, BeatmapSetLookupType.BeatmapId);

            req.Success += res =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    tcs.SetCanceled();
                    return;
                }

                tcs.SetResult(res.ToBeatmapSet(Rulesets));
            };

            req.Failure += e => tcs.SetException(e);

            API.Queue(req);

            return tcs.Task;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            connector?.Dispose();
        }
    }
}
