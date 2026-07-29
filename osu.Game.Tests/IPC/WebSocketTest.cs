// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Game.IPC;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Tests.IPC
{
    [TestFixture]
    public class WebSocketTest
    {
        [Test]
        public async Task TestClientInitiatedDuplexCommunication()
        {
            const int port = 54321;

            var server = new WebSocketServer(port);
            var client = new WebSocketClient(port);

            var duplexComplete = new ManualResetEventSlim(false);

            server.MessageReceived += (clientId, msg) =>
            {
                if (msg != "PING")
                    return;

                // ReSharper disable once AccessToDisposedClosure
                server.SendAsync(clientId, "PONG").FireAndForget();
            };
            client.MessageReceived += msg =>
            {
                if (msg != "PONG")
                    return;

                duplexComplete.Set();
            };

            await server.StartAsync();
            await client.Start();

            await client.SendAsync("PING");
            Assert.That(duplexComplete.Wait(10_000));

            await client.StopAsync();
            await server.StopAsync();

            client.Dispose();
            server.Dispose();
        }

        [Test]
        public async Task TestServerInitiatedDuplexCommunication()
        {
            const int port = 54321;

            var server = new WebSocketServer(port);
            var client = new WebSocketClient(port);

            var clientConnected = new ManualResetEventSlim();
            var duplexComplete = new ManualResetEventSlim();

            client.MessageReceived += msg =>
            {
                if (msg != "PING")
                    return;

                // ReSharper disable once AccessToDisposedClosure
                client.SendAsync("PONG").FireAndForget();
            };
            server.ClientConnected += _ => clientConnected.Set();
            server.MessageReceived += (_, msg) =>
            {
                if (msg != "PONG")
                    return;

                duplexComplete.Set();
            };

            await server.StartAsync();
            await client.Start();
            Assert.That(clientConnected.Wait(10_000));

            await server.SendAsync(1, "PING");
            Assert.That(duplexComplete.Wait(10_000));

            await client.StopAsync();
            await server.StopAsync();

            client.Dispose();
            server.Dispose();
        }

        [Test]
        public async Task TestServerBroadcast()
        {
            const int port = 54321;
            const int client_count = 5;

            var server = new WebSocketServer(port);
            var clients = new List<WebSocketClient>(client_count);
            var connectionCountdown = new CountdownEvent(client_count);
            var receiptCountdown = new CountdownEvent(client_count);

            for (int i = 0; i < client_count; ++i)
            {
                var client = new WebSocketClient(port);
                client.MessageReceived += msg =>
                {
                    if (msg != "HI ALL")
                        return;

                    receiptCountdown.Signal();
                };
                clients.Add(client);
            }

            server.ClientConnected += _ => connectionCountdown.Signal();

            await server.StartAsync();

            foreach (var client in clients)
                await client.Start();
            Assert.That(connectionCountdown.Wait(10_000));

            await server.BroadcastAsync("HI ALL");
            Assert.That(receiptCountdown.Wait(10_000));

            foreach (var client in clients)
            {
                await client.StopAsync();
                client.Dispose();
            }

            await server.StopAsync();
            server.Dispose();
        }

        [Test]
        public async Task TestClientSoftAborts()
        {
            const int port = 54321;

            var server = new WebSocketServer(port);
            var client = new WebSocketClient(port);

            await server.StartAsync();
            await client.Start();

            await client.StopAsync();
            client.Dispose();

            await server.StopAsync();
            server.Dispose();
        }

        [Test]
        public async Task TestClientHardAborts()
        {
            const int port = 54321;

            var server = new WebSocketServer(port);
            var client = new WebSocketClient(port);

            await server.StartAsync();
            await client.Start();

            await client.StopAsync(new CancellationToken(true));
            client.Dispose();

            await server.StopAsync();
            server.Dispose();
        }

        [Test]
        public async Task TestServerSoftAborts()
        {
            const int port = 54321;

            var server = new WebSocketServer(port);
            var client = new WebSocketClient(port);

            await server.StartAsync();
            await client.Start();

            await server.StopAsync();
            server.Dispose();

            await client.StopAsync();
            client.Dispose();
        }

        [Test]
        public async Task TestServerHardAborts()
        {
            const int port = 54321;

            var server = new WebSocketServer(port);
            var client = new WebSocketClient(port);

            await server.StartAsync();
            await client.Start();

            await server.StopAsync(new CancellationToken(true));
            server.Dispose();

            await client.StopAsync();
            client.Dispose();
        }

        [Test]
        public async Task TestClientMessageTooLong()
        {
            const int port = 54321;

            var server = new WebSocketServer(port);
            var client = new WebSocketClient(port);

            var clientClosed = new ManualResetEventSlim();
            client.Closed += clientClosed.Set;

            await server.StartAsync();
            await client.Start();

            await client.SendAsync(new string('0', 9999));
            Assert.That(clientClosed.Wait(10_000));
            await client.StopAsync();
            client.Dispose();

            var client2 = new WebSocketClient(port);

            var duplexComplete = new ManualResetEventSlim();
            server.MessageReceived += (clientId, msg) =>
            {
                if (msg != "PING")
                    return;

                // ReSharper disable once AccessToDisposedClosure
                server.SendAsync(clientId, "PONG").FireAndForget();
            };
            client2.MessageReceived += msg =>
            {
                if (msg != "PONG")
                    return;

                duplexComplete.Set();
            };

            await client2.Start();
            await client2.SendAsync("PING");
            Assert.That(duplexComplete.Wait(10000));

            await client2.StopAsync();
            await server.StopAsync();

            client2.Dispose();
            server.Dispose();
        }

        [Test]
        public async Task TestStartStopServerWithoutReceivingClients()
        {
            const int port = 54321;

            var server = new WebSocketServer(port);
            await server.StartAsync();
            await server.StopAsync();
            server.Dispose();
        }
    }
}
