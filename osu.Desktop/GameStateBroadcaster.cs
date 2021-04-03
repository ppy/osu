// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Desktop
{
    internal class GameStateBroadcaster : Component
    {
        private readonly List<WebSocketClient> clients = new List<WebSocketClient>();

        private HttpListener listener = new HttpListener();

        private CancellationTokenSource cancellationToken;

        private IBindable<User> user;

        private readonly Bindable<bool> enabled = new Bindable<bool>();

        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        private readonly Bindable<UserActivity> activity = new Bindable<UserActivity>();

        private ScheduledDelegate broadcastSchedule;

        public GameStateBroadcaster()
        {
            listener.Prefixes.Add($"http://localhost:7270/");
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, IAPIProvider provider, Bindable<RulesetInfo> ruleset, Bindable<WorkingBeatmap> beatmap)
        {
            this.ruleset.BindTo(ruleset);
            this.beatmap.BindTo(beatmap);

            (user = provider.LocalUser.GetBoundCopy()).BindValueChanged(u =>
            {
               activity.UnbindBindings();
               activity.BindTo(u.NewValue.Activity);
            });

            this.ruleset.ValueChanged += _ => broadcast();
            this.beatmap.ValueChanged += _ => broadcast();
            activity.ValueChanged += _ => broadcast();

            config.BindWith(OsuSetting.PublishGameState, enabled);

            enabled.BindValueChanged(state =>
            {
                if (state.NewValue)
                    Task.Factory.StartNew(start, TaskCreationOptions.LongRunning);
                else
                    stop();
            }, true);
        }

        private async Task start()
        {
            cancellationToken = new CancellationTokenSource();
            listener.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var context = await listener.GetContextAsync().ConfigureAwait(false);
                    var wsContext = await context.AcceptWebSocketAsync(null).ConfigureAwait(false);

                    var client = new WebSocketClient(wsContext.WebSocket);
                    client.OnStart += add;
                    client.OnClose += remove;

                    client.Start();
                }
                catch (HttpListenerException)
                {
                }
            }
        }

        private void add(WebSocketClient client)
        {
            lock (clients)
                clients.Add(client);
        }

        private void remove(WebSocketClient client)
        {
            lock (clients)
                clients.Remove(client);
        }

        private void broadcast()
        {
            if (!enabled.Value)
                return;

            broadcastSchedule?.Cancel();
            broadcastSchedule = Scheduler.AddDelayed(() =>
            {
                string state = JsonConvert.SerializeObject(new GameState
                {
                    Ruleset = ruleset.Value,
                    Activity = activity.Value,
                    BeatmapMetadata = beatmap.Value.Metadata,
                });

                foreach (var client in clients)
                    client.Enqueue(state);
            }, 200);
        }

        private void stop(bool closing = false)
        {
            cancellationToken?.Cancel();
            broadcastSchedule?.Cancel();
            Task.Run(async () =>
            {
                await Task.WhenAll(clients.ToArray().Select(c => c.Close())).ConfigureAwait(false);

                if (listener.IsListening)
                    listener.Stop();

                if (closing)
                    listener.Close();
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            stop(true);
        }

        private sealed class WebSocketClient
        {
            private readonly WebSocket socket;

            private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();

            private readonly BlockingCollection<string> queue = new BlockingCollection<string>();

            public event Action<WebSocketClient> OnClose;

            public event Action<WebSocketClient> OnStart;

            public WebSocketClient(WebSocket socket)
            {
                this.socket = socket;
            }

            public void Start()
            {
                OnStart?.Invoke(this);
                Task.WhenAll(receive(), send()).ConfigureAwait(false);
            }

            public void Enqueue(string message)
            {
                try
                {
                    queue.Add(message, cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                }
            }

            public async Task Close()
            {
                if (socket.State != WebSocketState.Open)
                    return;

                cancellationToken.Cancel();
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).ConfigureAwait(false);

                OnClose?.Invoke(this);

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
