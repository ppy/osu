// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using osu.Framework.Logging;

namespace osu.Desktop.GameState
{
    public class GameStateServer
    {
        private WebApplication? app;

        private IHubContext<GameStateHub, IGameStateClient>? hubContext;

        public bool IsRunning { get; private set; }

        public OnServerReady? OnReady;

        public OnServerStopped? OnStopped;

        public void Start()
        {
            if (IsRunning)
            {
                Logger.Log("Attempted to start game state server while it was already running.");
                return;
            }

            var builder = WebApplication.CreateBuilder();
            builder.Services.AddSignalR();

            // block chosen arbitrarily from list of unassigned ports
            // at https://www.iana.org/assignments/service-names-port-numbers/service-names-port-numbers.xhtml
            int port = findOpenPort(31000, 10);
            string serverUrl = $"http://localhost:{port}";
            builder.WebHost.UseUrls(serverUrl);

            app = builder.Build();
            app.MapHub<GameStateHub>("/gamestate");

            hubContext = app.Services.GetService<IHubContext<GameStateHub, IGameStateClient>>();

            var thread = new Thread(() => app.Run())
            {
                Name = "GameStateServer",
                IsBackground = true,
            };

            app.Lifetime.ApplicationStarted.Register(() =>
            {
                IsRunning = true;
                OnReady?.Invoke(serverUrl, hubContext!);
            });

            thread.Start();
        }

        public void Stop()
        {
            if (!IsRunning || app == null)
            {
                Logger.Log("Attempted to stop game state server while it was not running.");
                return;
            }

            Task.Run(() =>
            {
                app.Lifetime.StopApplication();

                IsRunning = false;
                OnStopped?.Invoke();
            });
        }

        private int findOpenPort(int start, int count)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            List<int> tcpEndpoints = properties.GetActiveTcpListeners().Select(endpoint => endpoint.Port).ToList();
            List<int> udpEndpoints = properties.GetActiveUdpListeners().Select(endpoint => endpoint.Port).ToList();

            int? port = Enumerable.Range(start, count).FirstOrDefault(port => !tcpEndpoints.Contains(port) && !udpEndpoints.Contains(port));

            return port ?? throw new InvalidOperationException($"No open port was found in the range provided ({start}-{start + count}.");
        }
    }

    public delegate void OnServerReady(string serverUrl, IHubContext<GameStateHub, IGameStateClient> hubContext);

    public delegate void OnServerStopped();
}
