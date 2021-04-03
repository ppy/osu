// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Desktop
{
    public class GameStateBroadcaster : Component
    {
        private readonly ConcurrentBag<WebSocketClient> clients = new ConcurrentBag<WebSocketClient>();

        private readonly HttpListener listener = new HttpListener();

        private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();

        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        private ScheduledDelegate broadcastSchedule;

        public GameStateBroadcaster()
        {
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();

            var thread = new Thread(() => handleConnections().Wait())
            {
                Name = "GameStateBroadcaster",
                IsBackground = true,
            };

            thread.Start();
        }

        [BackgroundDependencyLoader]
        private void load(Bindable<RulesetInfo> ruleset, Bindable<WorkingBeatmap> beatmap)
        {
            this.ruleset.BindTo(ruleset);
            this.beatmap.BindTo(beatmap);

            this.ruleset.ValueChanged += _ => broadcast();
            this.beatmap.ValueChanged += _ => broadcast();
        }

        private void broadcast()
        {
            broadcastSchedule?.Cancel();
            broadcastSchedule = Scheduler.AddDelayed(() =>
            {
                string state = JsonConvert.SerializeObject(new GameState
                {
                    Ruleset = ruleset.Value,
                    BeatmapMetadata = beatmap.Value.Metadata,
                });

                foreach (var client in clients)
                    client.Enqueue(state);
            }, 200);
        }

        public async Task Close()
        {
            foreach (var client in clients)
                await client.Close().ConfigureAwait(false);
        }

        private async Task handleConnections()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var context = await listener.GetContextAsync().ConfigureAwait(false);
                var wsContext = await context.AcceptWebSocketAsync(null).ConfigureAwait(false);

                var client = new WebSocketClient(wsContext.WebSocket);
                clients.Add(client);

                var thread = new Thread(() => client.Handle().Wait())
                {
                    Name = $"GameStateBroadcaster_Socket-{clients.Count}",
                    IsBackground = true,
                };

                thread.Start();
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Close().Wait();
        }

        [Serializable]
        public class GameState
        {
            public RulesetInfo Ruleset { get; set; }

            public BeatmapMetadata BeatmapMetadata { get; set; }
        }

        private sealed class WebSocketClient
        {
            private readonly WebSocket socket;

            private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();

            private readonly BlockingCollection<string> queue = new BlockingCollection<string>();

            public WebSocketClient(WebSocket socket)
            {
                this.socket = socket;
            }

            public async Task Handle() => await Task.WhenAll(receive(), send()).ConfigureAwait(false);

            public void Enqueue(string message)
            {
                queue.Add(message, cancellationToken.Token);
            }

            public async Task Close()
            {
                if (socket.State != WebSocketState.Open)
                    return;

                cancellationToken.Cancel();
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).ConfigureAwait(false);

                cancellationToken.Dispose();
                socket.Dispose();
            }

            private async Task send()
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        if (socket.State == WebSocketState.Closed || socket.State == WebSocketState.Aborted)
                            break;

                        if (socket.State == WebSocketState.Open && queue.TryTake(out string message))
                            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, cancellationToken.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }

            private async Task receive()
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        if (socket.State == WebSocketState.Closed || socket.State == WebSocketState.Aborted)
                            break;

                        var buffer = WebSocket.CreateServerBuffer(4096);
                        var received = await socket.ReceiveAsync(buffer, cancellationToken.Token).ConfigureAwait(false);

                        if (socket.State == WebSocketState.CloseReceived && received.MessageType == WebSocketMessageType.Close)
                            await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }
        }
    }
}
