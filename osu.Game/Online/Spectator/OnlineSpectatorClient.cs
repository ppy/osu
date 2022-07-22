// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Online.Spectator
{
    public class OnlineSpectatorClient : SpectatorClient
    {
        private readonly string endpoint;

        private IHubClientConnector? connector;

        public override IBindable<bool> IsConnected { get; } = new BindableBool();

        private HubConnection? connection => connector?.CurrentConnection;

        public OnlineSpectatorClient(EndpointConfiguration endpoints)
        {
            endpoint = endpoints.SpectatorEndpointUrl;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            connector = api.GetHubConnector(nameof(SpectatorClient), endpoint);

            if (connector != null)
            {
                connector.ConfigureConnection = connection =>
                {
                    // until strong typed client support is added, each method must be manually bound
                    // (see https://github.com/dotnet/aspnetcore/issues/15198)
                    connection.On<int, SpectatorState>(nameof(ISpectatorClient.UserBeganPlaying), ((ISpectatorClient)this).UserBeganPlaying);
                    connection.On<int, FrameDataBundle>(nameof(ISpectatorClient.UserSentFrames), ((ISpectatorClient)this).UserSentFrames);
                    connection.On<int, SpectatorState>(nameof(ISpectatorClient.UserFinishedPlaying), ((ISpectatorClient)this).UserFinishedPlaying);
                };

                IsConnected.BindTo(connector.IsConnected);
            }
        }

        protected override async Task BeginPlayingInternal(SpectatorState state)
        {
            if (!IsConnected.Value)
                return;

            Debug.Assert(connection != null);

            try
            {
                await connection.InvokeAsync(nameof(ISpectatorServer.BeginPlaySession), state);
            }
            catch (HubException exception)
            {
                if (exception.GetHubExceptionMessage() == HubClientConnector.SERVER_SHUTDOWN_MESSAGE)
                {
                    Debug.Assert(connector != null);

                    await connector.Reconnect();
                    await BeginPlayingInternal(state);
                }

                // Exceptions can occur if, for instance, the locally played beatmap doesn't have a server-side counterpart.
                // For now, let's ignore these so they don't cause unobserved exceptions to appear to the user (and sentry).
            }
        }

        protected override Task SendFramesInternal(FrameDataBundle bundle)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(ISpectatorServer.SendFrameData), bundle);
        }

        protected override Task EndPlayingInternal(SpectatorState state)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(ISpectatorServer.EndPlaySession), state);
        }

        protected override Task WatchUserInternal(int userId)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(ISpectatorServer.StartWatchingUser), userId);
        }

        protected override Task StopWatchingUserInternal(int userId)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(ISpectatorServer.EndWatchingUser), userId);
        }
    }
}
