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
    internal class GameStateBroadcaster : Component
    {
        private const double debounce_time = 100;

        private readonly IWebHost host;

        private ScheduledDelegate broadcastSchedule;

        private readonly List<WebSocketClient> clients = new List<WebSocketClient>();

        private readonly Bindable<bool> enabled = new Bindable<bool>();

        private readonly GameState state = new GameState();

        private IBindable<User> user;

        public GameStateBroadcaster()
        {
            host = new WebHostBuilder()
                .ConfigureServices(s =>
                {
                    s.AddSingleton(this);
                    s.AddTransient<WebSocketMiddleware>();
                })
                .UseKestrel()
                .UseUrls("http://localhost:7270")
                .UseStartup<Startup>()
                .Build();
        }

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

            state.Mods.ValueChanged += _ => broadcast();
            state.Ruleset.ValueChanged += _ => broadcast();
            state.Beatmap.ValueChanged += _ => broadcast();
            state.Activity.ValueChanged += _ => broadcast();

            config.BindWith(OsuSetting.PublishGameState, enabled);

            enabled.BindValueChanged(state =>
            {
                if (state.NewValue)
                    host.Start();
                else
                    stop();
            }, true);
        }

        public void AddWebSocketClient(WebSocketClient client)
        {
            lock (clients)
                clients.Add(client);
        }

        public void RemoveWebSocketClient(WebSocketClient client)
        {
            lock (clients)
                clients.Remove(client);
        }

        private void broadcast()
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

        private void stop()
        {
            broadcastSchedule?.Cancel();
            Task.Run(async () =>
            {
                IEnumerable<Task> closeTasks;
                lock (clients)
                    closeTasks = clients.Select(c => c.Close());

                await Task.WhenAll(closeTasks).ConfigureAwait(false);
                await host.StopAsync().ConfigureAwait(false);
            });
        }

        private class Startup
        {
            private readonly GameStateBroadcaster component;

            public Startup(GameStateBroadcaster component)
            {
                this.component = component;
            }

            public void Configure(IApplicationBuilder app)
            {
                app.UseWebSockets();
                app.UseMiddleware<WebSocketMiddleware>(component);
            }
        }

        public class WebSocketMiddleware
        {
            private readonly RequestDelegate next;

            private readonly GameStateBroadcaster component;

            public WebSocketMiddleware(RequestDelegate next, GameStateBroadcaster component)
            {
                this.component = component;
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
                        client.OnStart += component.AddWebSocketClient;
                        client.OnClose += component.RemoveWebSocketClient;
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

            public event Action<WebSocketClient> OnStart;

            public event Action<WebSocketClient> OnClose;

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
