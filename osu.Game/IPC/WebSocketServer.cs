// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Logging;

namespace osu.Game.IPC
{
    /// <summary>
    /// Implements a WebSocket server to be used for external integrations such as streaming overlays.
    /// The server can only listen on <c>localhost</c>, on the port given in the constructor.
    /// Only UTF-8 string-based messages are supported. Binary messages are not supported.
    /// String-based messages must not exceed <see cref="WebSocketChannel.max_message_size"/> bytes.
    /// </summary>
    /// <remarks>
    /// This implementation uses <see cref="HttpListener"/> internally.
    /// This is a frozen .NET API as per https://github.com/dotnet/runtime/issues/63941#issuecomment-1205259894.
    /// The reason of using this API instead of ASP.NET directly via frameworks like SignalR are as follows:
    /// <list type="bullet">
    /// <item>
    /// This is intended to be a <b>simple</b> server.
    /// There are no reliability guarantees, no delivery guarantees, no authorisation.
    /// The operation of this server is <b>best-effort</b>.
    /// Due to this, ASP.NET is surplus to requirements.
    /// </item>
    /// <item>Including ASP.NET wholesale would have a negative impact on binary size.</item>
    /// <item>
    /// Using ASP.NET could expose end users' PCs to having things enabled that shouldn't be enabled via little-known configuration toggles.
    /// One pertinent example is the <c>ASPNETCORE_URLS</c> environment variable which silently changes which endpoints an ASP.NET service listens on.
    /// </item>
    /// <item>
    /// ASP.NET does not generally fit into the paradigm of being <i>part</i> of an application.
    /// The way ASP.NET apps are structured, is that they generally <i>take over</i> the functioning of an application.
    /// Therefore, there is not necessarily a given that ASP.NET bundled inside the client will fully stop functioning even when explicitly asked.
    /// </item>
    /// </list>
    /// </remarks>
    public sealed class WebSocketServer : IDisposable
    {
        /// <summary>
        /// Whether the server is currently running and listening for connection requests.
        /// </summary>
        public bool IsRunning => handleRequestTask != null && !runningTokenSource.IsCancellationRequested;

        /// <summary>
        /// Invoked when a client is connected.
        /// The argument is the assigned ID of the client.
        /// </summary>
        public event Action<int>? ClientConnected;

        /// <summary>
        /// Invoked when a message is received.
        /// The first argument is the ID of the sender; the second is the content of the received message.
        /// </summary>
        public event Action<int, string>? MessageReceived;

        private readonly object syncRoot = new object();

        private readonly string prefix;
        private readonly Logger logger;

        private HttpListener? listener;
        private readonly ManualResetEventSlim contextResetEvent = new ManualResetEventSlim();
        private Task? handleRequestTask;

        private int channelCounter;
        private readonly ConcurrentDictionary<int, WebSocketChannel> channels = new ConcurrentDictionary<int, WebSocketChannel>();

        private readonly CancellationTokenSource runningTokenSource = new CancellationTokenSource();
        private bool isDisposed;

        public WebSocketServer(int port)
        {
            // Restricting to only providing a port is intentional for several reasons:
            // - Use of HTTP (no efforts are taken to make HTTPS work).
            // - Attack surface reduction (doesn't accidentally listen on all interfaces, potentially getting hit by something external).
            // Some users with setups that use a second "streaming PC" or similar will complain. They can set up proxies at their own peril.
            prefix = $@"http://localhost:{port}/";

            logger = Logger.GetLogger(@"websocket");
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <param name="cancellationToken">Use this to cancel start-up.</param>
        public Task StartAsync(CancellationToken cancellationToken = default) => Task.Run(() =>
        {
            lock (syncRoot)
            {
                if (listener != null)
                    throw new InvalidOperationException($@"Cannot call {nameof(StartAsync)} multiple times.");

                listener = new HttpListener();
                listener.Prefixes.Add(prefix);
                listener.Start();
                handleRequestTask = Task.Run(handleRequests, cancellationToken);
                logger.Add($@"Listening on {prefix}.");
            }
        }, cancellationToken);

        private async Task handleRequests()
        {
            Debug.Assert(listener != null);

            while (!runningTokenSource.IsCancellationRequested)
            {
                HttpListenerContext? context = null;

                // `listener.GetContextAsync()` exists but is unusable here without ugly hacks.
                // as per source inspection, it is a thin wrapper over `{Begin,End}GetContext()`.
                // the problem with that is that the method is *hard-blocking* and *does not accept cancellation*.
                // therefore, if it's called in a processing loop like this
                // that we are expecting to be able to cut short at any moment's notice to shut things down,
                // it's not going to yield and will keep waiting forever.
                // a `listener.Stop()` from another thread does cut the call short, but also ends up in an unclean termination.
                // what "unclean termination" means here depends on the OS we're running on
                // (different exceptions are observed on macOS and Windows, at least).
                // therefore use the old asynchronous paradigm with manual signalling when the context is available.
                contextResetEvent.Reset();
                listener.BeginGetContext(iar =>
                {
                    try
                    {
                        context = ((HttpListener)iar.AsyncState!).EndGetContext(iar);
                        contextResetEvent.Set();
                    }
                    catch (HttpListenerException ex) when (ex.ErrorCode == 995)
                    {
                        // occurs on Windows when the listener is stopped.
                    }
                }, listener);
                WaitHandle.WaitAny([contextResetEvent.WaitHandle, runningTokenSource.Token.WaitHandle]);

                // either we have a context to use, or the cancellation fired.
                // if it's the latter, terminate processing loop.
                if (runningTokenSource.IsCancellationRequested)
                    return;

                Debug.Assert(context != null);

                var request = context.Request;
                var response = context.Response;

                if (!request.IsWebSocketRequest)
                {
                    logger.Add($@"Received non-websocket request from {request.RemoteEndPoint}. Requesting upgrade.");
                    response.StatusCode = (int)HttpStatusCode.UpgradeRequired;
                    response.Headers.Add(HttpRequestHeader.Upgrade, @"websocket");
                    response.Close();
                    continue;
                }

                HttpListenerWebSocketContext wsContext;

                try
                {
                    wsContext = await context.AcceptWebSocketAsync(null).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.Add($@"Failed to accept websocket connection from {request.RemoteEndPoint}.", LogLevel.Error, ex);
                    continue;
                }

                int channelId = Interlocked.Increment(ref channelCounter);
                var wsChannel = new WebSocketChannel(wsContext.WebSocket);
                channels[channelId] = wsChannel;
                wsChannel.MessageReceived += msg => MessageReceived?.Invoke(channelId, msg);
                wsChannel.ClosedPrematurely += () => onChannelClosed(channelId);
                wsChannel.Start(runningTokenSource.Token);
                logger.Add($@"Accepted websocket connection from {request.RemoteEndPoint} as client #{channelId}.");
                ClientConnected?.Invoke(channelId);
            }
        }

        private void onChannelClosed(int channelId)
        {
            if (channels.TryRemove(channelId, out var channel))
                channel.Dispose();
            logger.Add($@"Connection with client #{channelId} closed.");
        }

        /// <summary>
        /// Sends <paramref name="message"/> to the specific client with the given <paramref name="clientId"/>.
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="clientId"/> is not known.</exception>
        public async Task SendAsync(int clientId, string message)
        {
            if (!channels.TryGetValue(clientId, out var channel))
                throw new ArgumentException($@"Client {clientId} is not known.");

            logger.Add($@"Sending to client {clientId}: {message}");
            await channel.SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends <paramref name="message"/> to all connected clients.
        /// </summary>
        public Task BroadcastAsync(string message)
        {
            logger.Add($@"Broadcasting to all clients: {message}");
            return Task.WhenAll(channels.Values.Select(ch => ch.SendAsync(message)).ToArray());
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        /// <param name="stoppingToken">Cancel this to transition from a graceful shutdown to a forced shutdown.</param>
        public Task StopAsync(CancellationToken stoppingToken = default) => Task.Run(async () =>
        {
            if (isDisposed)
                return;

            logger.Add(@"Stopping websocket server...");

            // of note, ordering here is important - the token is supposed to be cancelled *before* the listener is stopped.
            // see `readWriteTask()` and the treatment of early cancellation for answer why.
            await runningTokenSource.CancelAsync().ConfigureAwait(false);

            if (handleRequestTask != null)
            {
                try
                {
                    await handleRequestTask.WaitAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // has to be caught manually because outer task isn't accepting `stoppingToken`.
                }
            }

            try
            {
                listener?.Stop();
            }
            catch (ObjectDisposedException)
            {
                // observed to intermittently fire on unices in unclear circumstances. tragic, but also irrelevant at this point. the point is to stop.
            }

            try
            {
                await Task.WhenAll(channels.Values.Select(ch => ch.StopAsync(stoppingToken)).ToArray()).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // has to be caught manually because outer task isn't accepting `stoppingToken`.
            }

            logger.Add(@"Websocket server stopped.");
        }, CancellationToken.None); // we always want this task to start running. passing `stoppingToken` here would mean potentially never even scheduling it for execution.

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            // no clue why this isn't accessible without casting.
            // sidebar: `Stop()` unregisters addresses on Windows, but `Abort()` doesn't!
            // this `Dispose()` implementation calls the former.
            (listener as IDisposable)?.Dispose();

            foreach (var channel in channels.Values)
                channel.Dispose();

            runningTokenSource.Dispose();
            contextResetEvent.Dispose();
        }
    }
}
