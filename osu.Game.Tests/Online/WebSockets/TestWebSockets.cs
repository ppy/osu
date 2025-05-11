// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Game.Online.WebSockets;

namespace osu.Game.Tests.Online.WebSockets
{
    [TestFixture]
    public class TestWebSockets
    {
        private TestWebSocketClient client = null!;
        private TestWebSocketServer server = null!;
        private static readonly Uri server_uri = new Uri(@"http://localhost:7270/");
        private static readonly Uri client_uri = new Uri(@"ws://localhost:7270/");

        [SetUp]
        public void SetUp()
        {
            client = new TestWebSocketClient(client_uri);
            server = new TestWebSocketServer();
            server.Start(server_uri);
        }

        [TearDown]
        public async Task TearDown()
        {
            await client.StopAsync().ConfigureAwait(false);
            await server.StopAsync().ConfigureAwait(false);
        }

        [Test]
        public async Task TestClientConnection()
        {
            await client.StartAsync().ConfigureAwait(false);
            await server.WaitForConnection();
            Assert.NotZero(server.Connected);
            await client.StopAsync().ConfigureAwait(false);
            await server.WaitForDisconnect();
            Assert.Zero(server.Connected);
        }

        [Test]
        public async Task TestServerConnection()
        {
            await client.StartAsync().ConfigureAwait(false);
            await server.WaitForConnection();
            Assert.NotZero(server.Connected);
            await server.StopAsync().ConfigureAwait(false);
            Assert.Zero(server.Connected);
        }

        [Test]
        public async Task TestServerMessage()
        {
            const string message = "Hello World";
            await client.StartAsync().ConfigureAwait(false);
            await server.SendAsync(message).ConfigureAwait(false);

            string received = await client.ReceiveAsync();
            Assert.AreEqual(message, received);
        }

        [Test]
        public async Task TestClientMessage()
        {
            const string message = "Hello World";
            await client.StartAsync().ConfigureAwait(false);
            await client.SendAsync(message).ConfigureAwait(false);

            string received = await server.ReceiveAsync();
            Assert.AreEqual(message, received);
        }

        private class TestWebSocketServer : WebSocketServer
        {
            private readonly TaskCompletionSource<string> receive = new TaskCompletionSource<string>();
            private readonly TaskCompletionSource connection = new TaskCompletionSource();
            private readonly TaskCompletionSource disconnect = new TaskCompletionSource();

            public Task<string> ReceiveAsync()
            {
                return receive.Task;
            }

            public Task WaitForConnection()
            {
                return connection.Task;
            }

            public Task WaitForDisconnect()
            {
                return disconnect.Task;
            }

            protected override Task OnMessage(int id, ReadOnlyMemory<byte> data, CancellationToken token = default)
            {
                receive.SetResult(Encoding.UTF8.GetString(data.Span));
                return base.OnMessage(id, data, token);
            }

            protected override Task OnConnect(int id, CancellationToken token = default)
            {
                connection.SetResult();
                return base.OnConnect(id, token);
            }

            protected override Task OnDisconnect(int id, CancellationToken token = default)
            {
                disconnect.SetResult();
                return base.OnDisconnect(id, token);
            }
        }

        private class TestWebSocketClient : WebSocketClient
        {
            private readonly TaskCompletionSource<string> receive = new TaskCompletionSource<string>();

            public TestWebSocketClient(Uri uri, IDictionary<string, string>? headers = null)
                : base(uri, headers)
            {
            }

            public Task<string> ReceiveAsync()
            {
                return receive.Task;
            }

            protected override Task OnMessage(ReadOnlyMemory<byte> data, CancellationToken token = default)
            {
                receive.SetResult(Encoding.UTF8.GetString(data.Span));
                return base.OnMessage(data, token);
            }
        }
    }
}
