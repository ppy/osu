// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using osu.Framework;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game.Online.API;

namespace osu.Game.Online
{
    public class HubClientConnector : IHubClientConnector
    {
        /// <summary>
        /// Invoked whenever a new hub connection is built, to configure it before it's started.
        /// </summary>
        public Action<HubConnection>? ConfigureConnection { get; set; }

        private readonly string clientName;
        private readonly string endpoint;
        private readonly string versionHash;
        private readonly bool preferMessagePack;
        private readonly IAPIProvider api;

        /// <summary>
        /// The current connection opened by this connector.
        /// </summary>
        public HubConnection? CurrentConnection { get; private set; }

        /// <summary>
        /// Whether this is connected to the hub, use <see cref="CurrentConnection"/> to access the connection, if this is <c>true</c>.
        /// </summary>
        public IBindable<bool> IsConnected => isConnected;

        private readonly Bindable<bool> isConnected = new Bindable<bool>();
        private readonly SemaphoreSlim connectionLock = new SemaphoreSlim(1);
        private CancellationTokenSource connectCancelSource = new CancellationTokenSource();

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        /// <summary>
        /// Constructs a new <see cref="HubClientConnector"/>.
        /// </summary>
        /// <param name="clientName">The name of the client this connector connects for, used for logging.</param>
        /// <param name="endpoint">The endpoint to the hub.</param>
        /// <param name="api"> An API provider used to react to connection state changes.</param>
        /// <param name="versionHash">The hash representing the current game version, used for verification purposes.</param>
        /// <param name="preferMessagePack">Whether to use MessagePack for serialisation if available on this platform.</param>
        public HubClientConnector(string clientName, string endpoint, IAPIProvider api, string versionHash, bool preferMessagePack = true)
        {
            this.clientName = clientName;
            this.endpoint = endpoint;
            this.api = api;
            this.versionHash = versionHash;
            this.preferMessagePack = preferMessagePack;

            apiState.BindTo(api.State);
            apiState.BindValueChanged(state =>
            {
                switch (state.NewValue)
                {
                    case APIState.Failing:
                    case APIState.Offline:
                        Task.Run(() => disconnect(true));
                        break;

                    case APIState.Online:
                        Task.Run(connect);
                        break;
                }
            }, true);
        }

        private async Task connect()
        {
            cancelExistingConnect();

            if (!await connectionLock.WaitAsync(10000).ConfigureAwait(false))
                throw new TimeoutException("Could not obtain a lock to connect. A previous attempt is likely stuck.");

            try
            {
                while (apiState.Value == APIState.Online)
                {
                    // ensure any previous connection was disposed.
                    // this will also create a new cancellation token source.
                    await disconnect(false).ConfigureAwait(false);

                    // this token will be valid for the scope of this connection.
                    // if cancelled, we can be sure that a disconnect or reconnect is handled elsewhere.
                    var cancellationToken = connectCancelSource.Token;

                    cancellationToken.ThrowIfCancellationRequested();

                    Logger.Log($"{clientName} connecting...", LoggingTarget.Network);

                    try
                    {
                        // importantly, rebuild the connection each attempt to get an updated access token.
                        CurrentConnection = buildConnection(cancellationToken);

                        await CurrentConnection.StartAsync(cancellationToken).ConfigureAwait(false);

                        Logger.Log($"{clientName} connected!", LoggingTarget.Network);
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
                        await handleErrorAndDelay(e, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                connectionLock.Release();
            }
        }

        /// <summary>
        /// Handles an exception and delays an async flow.
        /// </summary>
        private async Task handleErrorAndDelay(Exception exception, CancellationToken cancellationToken)
        {
            Logger.Log($"{clientName} connection error: {exception}", LoggingTarget.Network);
            await Task.Delay(5000, cancellationToken).ConfigureAwait(false);
        }

        private HubConnection buildConnection(CancellationToken cancellationToken)
        {
            var builder = new HubConnectionBuilder()
                .WithUrl(endpoint, options =>
                {
                    options.Headers.Add("Authorization", $"Bearer {api.AccessToken}");
                    options.Headers.Add("OsuVersionHash", versionHash);
                });

            if (RuntimeInfo.SupportsJIT && preferMessagePack)
            {
                builder.AddMessagePackProtocol(options =>
                {
                    options.SerializerOptions = SignalRUnionWorkaroundResolver.OPTIONS;
                });
            }
            else
            {
                // eventually we will precompile resolvers for messagepack, but this isn't working currently
                // see https://github.com/neuecc/MessagePack-CSharp/issues/780#issuecomment-768794308.
                builder.AddNewtonsoftJsonProtocol(options =>
                {
                    options.PayloadSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.PayloadSerializerSettings.Converters = new List<JsonConverter>
                    {
                        new SignalRDerivedTypeWorkaroundJsonConverter(),
                    };
                });
            }

            var newConnection = builder.Build();

            ConfigureConnection?.Invoke(newConnection);

            newConnection.Closed += ex => onConnectionClosed(ex, cancellationToken);
            return newConnection;
        }

        private async Task onConnectionClosed(Exception? ex, CancellationToken cancellationToken)
        {
            isConnected.Value = false;

            if (ex != null)
                await handleErrorAndDelay(ex, cancellationToken).ConfigureAwait(false);
            else
                Logger.Log($"{clientName} disconnected", LoggingTarget.Network);

            // make sure a disconnect wasn't triggered (and this is still the active connection).
            if (!cancellationToken.IsCancellationRequested)
                await Task.Run(connect, default).ConfigureAwait(false);
        }

        private async Task disconnect(bool takeLock)
        {
            cancelExistingConnect();

            if (takeLock)
            {
                if (!await connectionLock.WaitAsync(10000).ConfigureAwait(false))
                    throw new TimeoutException("Could not obtain a lock to disconnect. A previous attempt is likely stuck.");
            }

            try
            {
                if (CurrentConnection != null)
                    await CurrentConnection.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                CurrentConnection = null;
                if (takeLock)
                    connectionLock.Release();
            }
        }

        private void cancelExistingConnect()
        {
            connectCancelSource.Cancel();
            connectCancelSource = new CancellationTokenSource();
        }

        public override string ToString() => $"Connector for {clientName} ({(IsConnected.Value ? "connected" : "not connected")}";

        public void Dispose()
        {
            apiState.UnbindAll();
            cancelExistingConnect();
        }
    }
}
