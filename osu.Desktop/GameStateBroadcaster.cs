// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Users;

namespace osu.Desktop
{
    public class GameStateBroadcaster : Component
    {
        private const double debounce_time = 100;

        private IWebHost host;

        private ScheduledDelegate broadcastSchedule;

        private readonly List<WebSocketClient> clients = new List<WebSocketClient>();

        private readonly Bindable<bool> enabled = new Bindable<bool>();

        private readonly GameState state = new GameState();

        private IBindable<User> user;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, IAPIProvider provider, Bindable<RulesetInfo> ruleset, Bindable<WorkingBeatmap> beatmap, Bindable<IReadOnlyList<Mod>> mods)
        {
            state.Ruleset.BindTo(ruleset);
            state.Beatmap.BindTo(beatmap);
            state.Mods.BindTo(mods);

            user = provider.LocalUser.GetBoundCopy();
            user.BindValueChanged(u =>
            {
                state.Activity.UnbindBindings();
                state.Activity.BindTo(u.NewValue.Activity);
            }, true);

            state.Mods.ValueChanged += _ => Broadcast();
            state.Ruleset.ValueChanged += _ => Broadcast();
            state.Beatmap.ValueChanged += _ => Broadcast();
            state.Activity.ValueChanged += _ => Broadcast();

            config.BindWith(OsuSetting.PublishGameState, enabled);

            enabled.BindValueChanged(state =>
            {
                if (state.NewValue)
                    start();
                else
                    stop();
            }, true);
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

        public void Broadcast()
        {
            broadcastSchedule?.Cancel();
            broadcastSchedule = Scheduler.AddDelayed(() =>
            {
                lock (clients)
                {
                    foreach (var client in clients)
                        client.Enqueue(getStateAsString());
                }
            }, debounce_time);
        }

        private string getStateAsString()
        {
            return JsonConvert.SerializeObject(state, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        private void start()
        {
            if (host != null)
                return;

            host = new WebHostBuilder()
                .ConfigureServices(s =>
                {
                    s.AddTransient<WebSocketStart>(p => add);
                    s.AddTransient<WebSocketClose>(p => remove);
                    s.AddTransient<WebSocketReady>(p => () => Broadcast());
                    s.AddTransient<WebSocketMiddleware>();
                })
                .UseKestrel()
                .UseUrls("http://localhost:7270")
                .UseStartup<Startup>()
                .Build();

            host.Start();
        }

        private void stop()
        {
            enabled.Disabled = true;
            broadcastSchedule?.Cancel();
            Task.Run(async () =>
            {
                IEnumerable<Task> closeTasks;
                lock (clients)
                    closeTasks = clients.Select(c => c.Close());

                await Task.WhenAll(closeTasks).ConfigureAwait(false);
                await host.StopAsync().ConfigureAwait(false);

                host = null;
                enabled.Disabled = false;
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            stop();
        }

        public delegate void WebSocketStart(WebSocketClient client);

        public delegate void WebSocketClose(WebSocketClient client);

        public delegate void WebSocketReady();

        private class Startup
        {
            private readonly WebSocketStart start;

            private readonly WebSocketClose close;

            private readonly WebSocketReady ready;

            public Startup(WebSocketStart start, WebSocketClose close, WebSocketReady ready)
            {
                this.start = start;
                this.close = close;
                this.ready = ready;
            }

            public void Configure(IApplicationBuilder app)
            {
                app.UseWebSockets();
                app.UseMiddleware<WebSocketMiddleware>(start, close, ready);
            }
        }

        public class WebSocketMiddleware
        {
            private readonly RequestDelegate next;

            private readonly WebSocketStart start;

            private readonly WebSocketClose close;

            private readonly WebSocketReady ready;

            public WebSocketMiddleware(RequestDelegate next, WebSocketStart start, WebSocketClose close, WebSocketReady ready)
            {
                this.start = start;
                this.close = close;
                this.ready = ready;
                this.next = next;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                try
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var socket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                        var client = new WebSocketClient(socket);
                        client.OnStart = new Action<WebSocketClient>(start);
                        client.OnClose = new Action<WebSocketClient>(close);
                        client.OnReady = new Action(ready);
                        client.Start();

                        await client.CompletionSource.Task.ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"An exception occured in the WebSocket\n{e.GetType().Name}\n{e.StackTrace}", LoggingTarget.Network);
                    throw;
                }

                await next(context).ConfigureAwait(false);
            }
        }

        public class WebSocketClient
        {
            private readonly WebSocket socket;

            private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();

            private readonly Queue<string> queue = new Queue<string>();

            public Action<WebSocketClient> OnStart;

            public Action<WebSocketClient> OnClose;

            public Action OnReady;

            private bool isReady;

            public readonly TaskCompletionSource<bool> CompletionSource = new TaskCompletionSource<bool>();

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
                lock (queue)
                    queue.Enqueue(message);
            }

            public async Task Close(bool graceful = false)
            {
                if (graceful)
                    await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).ConfigureAwait(false);
                else
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).ConfigureAwait(false);

                cancellationToken.Cancel();
                cancellationToken.Dispose();

                OnClose?.Invoke(this);
                socket.Dispose();

                CompletionSource.SetResult(true);
            }

            private async Task send()
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        if (socket.State == WebSocketState.Closed || socket.State == WebSocketState.Aborted)
                            break;

                        bool success = false;
                        string message = null;

                        lock (queue)
                            success = queue.TryDequeue(out message);

                        if (socket.State == WebSocketState.Open && success)
                            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, cancellationToken.Token).ConfigureAwait(false);

                        if (!isReady)
                        {
                            OnReady?.Invoke();
                            isReady = true;
                        }
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
                            await Close(true).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }
        }
    }
}
