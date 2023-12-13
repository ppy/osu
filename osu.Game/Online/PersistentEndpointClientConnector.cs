// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Logging;
using osu.Game.Online.API;

namespace osu.Game.Online
{
    public abstract class PersistentEndpointClientConnector : IDisposable
    {
        /// <summary>
        /// Whether the managed connection is currently connected. When <c>true</c> use <see cref="CurrentConnection"/> to access the connection.
        /// </summary>
        public IBindable<bool> IsConnected => isConnected;

        /// <summary>
        /// The current connection opened by this connector.
        /// </summary>
        public PersistentEndpointClient? CurrentConnection { get; private set; }

        protected readonly IAPIProvider API;

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();
        private readonly Bindable<bool> isConnected = new Bindable<bool>();
        private readonly SemaphoreSlim connectionLock = new SemaphoreSlim(1);
        private CancellationTokenSource connectCancelSource = new CancellationTokenSource();
        private bool started;

        /// <summary>
        /// Constructs a new <see cref="PersistentEndpointClientConnector"/>.
        /// </summary>
        /// <param name="api"> An API provider used to react to connection state changes.</param>
        protected PersistentEndpointClientConnector(IAPIProvider api)
        {
            API = api;
            apiState.BindTo(api.State);
        }

        /// <summary>
        /// Attempts to connect and begins processing messages from the remote endpoint.
        /// </summary>
        public void Start()
        {
            if (started)
                return;

            apiState.BindValueChanged(_ => Task.Run(connectIfPossible), true);
            started = true;
        }

        public Task Reconnect()
        {
            Logger.Log($"{ClientName} reconnecting...", LoggingTarget.Network);
            return Task.Run(connectIfPossible);
        }

        private async Task connectIfPossible()
        {
            switch (apiState.Value)
            {
                case APIState.Failing:
                case APIState.Offline:
                    await disconnect(true).ConfigureAwait(true);
                    break;

                case APIState.Online:
                    await connect().ConfigureAwait(true);
                    break;
            }
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

                    Logger.Log($"{ClientName} connecting...", LoggingTarget.Network);

                    try
                    {
                        // importantly, rebuild the connection each attempt to get an updated access token.
                        CurrentConnection = await BuildConnectionAsync(cancellationToken).ConfigureAwait(false);
                        CurrentConnection.Closed += ex => onConnectionClosed(ex, cancellationToken);

                        cancellationToken.ThrowIfCancellationRequested();

                        await CurrentConnection.ConnectAsync(cancellationToken).ConfigureAwait(false);

                        Logger.Log($"{ClientName} connected!", LoggingTarget.Network);
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
            Logger.Log($"{ClientName} connect attempt failed: {exception.Message}", LoggingTarget.Network);
            await Task.Delay(5000, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new <see cref="PersistentEndpointClient"/>.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to stop the process.</param>
        protected abstract Task<PersistentEndpointClient> BuildConnectionAsync(CancellationToken cancellationToken);

        private async Task onConnectionClosed(Exception? ex, CancellationToken cancellationToken)
        {
            bool hasBeenCancelled = cancellationToken.IsCancellationRequested;

            await disconnect(true).ConfigureAwait(false);

            if (ex != null)
                await handleErrorAndDelay(ex, CancellationToken.None).ConfigureAwait(false);
            else
                Logger.Log($"{ClientName} disconnected", LoggingTarget.Network);

            // make sure a disconnect wasn't triggered (and this is still the active connection).
            if (!hasBeenCancelled)
                await Task.Run(connect, default).ConfigureAwait(false);
        }

        protected Task Disconnect() => disconnect(true);

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
                isConnected.Value = false;
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

        protected virtual string ClientName => GetType().ReadableName();

        public override string ToString() => $"{ClientName} ({(IsConnected.Value ? "connected" : "not connected")})";

        private bool isDisposed;

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposed)
                return;

            apiState.UnbindAll();
            cancelExistingConnect();

            isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
