// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace osu.Game.IPC
{
    /// <summary>
    /// Represents a WebSocket-based communication channel.
    /// Only supports UTF-8 string-based messages, of maximum size of <see cref="max_message_size"/> bytes.
    /// </summary>
    public sealed class WebSocketChannel : IDisposable
    {
        public event Action<string>? MessageReceived;
        public event Action? ClosedPrematurely;

        private const int max_message_size = 4096; // bytes

        private readonly byte[] receiveBuffer = new byte[max_message_size];
        private int currentBufferPosition;

        private readonly WebSocket webSocket;
        private Task? readWriteTask;
        private readonly CancellationTokenSource runningTokenSource = new CancellationTokenSource();
        private bool isDisposed;

        public WebSocketChannel(WebSocket webSocket)
        {
            this.webSocket = webSocket;
        }

        /// <summary>
        /// Starts the channel.
        /// </summary>
        /// <param name="cancellationToken">Use this to abort the start.</param>
        public void Start(CancellationToken cancellationToken)
        {
            if (readWriteTask?.Status >= TaskStatus.Running)
                throw new InvalidOperationException($@"Cannot {nameof(Start)} more than once.");

            readWriteTask = Task.Run(readWriteLoop, cancellationToken);
        }

        private async Task readWriteLoop()
        {
            var token = runningTokenSource.Token;

            while (!token.IsCancellationRequested)
            {
                ValueWebSocketReceiveResult result;

                try
                {
                    result = await webSocket.ReceiveAsync(receiveBuffer.AsMemory(currentBufferPosition), token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // normal when `token` is cancelled.
                    // at this point the websocket will have entered `Aborted` state on its own, so no further clean-up can be done.
                    return;
                }
                catch (Exception)
                {
                    // could throw something like `WebSocketException`s from the other side hard-aborting.
                    ClosedPrematurely?.Invoke();
                    return;
                }

                currentBufferPosition += result.Count;

                if (webSocket.State > WebSocketState.Open)
                {
                    if (webSocket.State == WebSocketState.CloseReceived)
                    {
                        try
                        {
                            // attempt to complete the close handshake nicely.
                            await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, @"Received close request", token).ConfigureAwait(false);
                        }
                        catch
                        {
                            // an attempt was made, and failed. bail.
                        }
                    }

                    ClosedPrematurely?.Invoke();
                    return;
                }

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    // see https://github.com/dotnet/runtime/issues/81762#issuecomment-1421029475 for difference between `CloseAsync()` and `CloseOutputAsync()`.
                    // there is basically no incentive to use `CloseAsync()` in these error scenarios. the point is to drop the errant peer on the floor immediately.
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.InvalidMessageType, @"Binary messages are not supported.", token).ConfigureAwait(false);
                    ClosedPrematurely?.Invoke();
                    return;
                }

                if (currentBufferPosition >= max_message_size)
                {
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.MessageTooBig, $@"Exceeded maximum message size of {max_message_size} bytes.", token).ConfigureAwait(false);
                    ClosedPrematurely?.Invoke();
                    return;
                }

                if (result.EndOfMessage)
                {
                    string message;

                    try
                    {
                        message = Encoding.UTF8.GetString(receiveBuffer, 0, currentBufferPosition);
                    }
                    catch (ArgumentException)
                    {
                        await webSocket.CloseOutputAsync(WebSocketCloseStatus.InvalidPayloadData, @"UTF-8 encoded strings expected.", token).ConfigureAwait(false);
                        ClosedPrematurely?.Invoke();
                        return;
                    }

                    MessageReceived?.Invoke(message);
                    Array.Fill(receiveBuffer, (byte)0, 0, currentBufferPosition);
                    currentBufferPosition = 0;
                }
            }
        }

        public async Task SendAsync(string message)
        {
            if (readWriteTask == null)
                throw new InvalidOperationException($@"Must {nameof(Start)} first.");

            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Stops the channel.
        /// </summary>
        /// <param name="stoppingToken">Cancel this to transition from a graceful shutdown to a forced shutdown.</param>
        public async Task StopAsync(CancellationToken stoppingToken)
        {
            if (isDisposed)
                return;

            await runningTokenSource.CancelAsync().ConfigureAwait(false);

            if (readWriteTask != null)
                await readWriteTask.WaitAsync(stoppingToken).ConfigureAwait(false);

            if (stoppingToken.IsCancellationRequested)
                webSocket.Abort();
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            webSocket.Dispose();
            runningTokenSource.Dispose();
        }
    }
}
