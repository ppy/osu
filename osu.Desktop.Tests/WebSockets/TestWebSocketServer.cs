// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Desktop.WebSockets;

namespace osu.Desktop.Tests.WebSockets
{
    [TestFixture]
    public class TestWebSocketServer
    {
        private ClientWebSocket client;
        private ServerWebSocket server;

        [SetUp]
        public async Task SetUp()
        {
            client = new ClientWebSocket();
            server = new ServerWebSocket();

            await server.Start().ConfigureAwait(false);
            await client.ConnectAsync(new Uri("ws://localhost:7270/test/"), CancellationToken.None).ConfigureAwait(false);
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
            client = null;

            server.Dispose();
            server = null;
        }

        [Test]
        public void TestServerMessage()
        {
            const string message = "Hello World";
            var received = new ArraySegment<byte>(new byte[4096]);
            client.ReceiveAsync(received, CancellationToken.None);
            server.Broadcast(message);
            Thread.Sleep(500);
            Assert.AreEqual(message, Encoding.UTF8.GetString(received.Slice(0, message.Length).AsSpan()));
        }

        [Test]
        public async Task TestClientMessage()
        {
            var sent = new ArraySegment<byte>(Encoding.UTF8.GetBytes("Hello World"));
            await client.SendAsync(sent, WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
            await Task.Delay(500).ConfigureAwait(false);
            Assert.AreEqual("Hello World", server.LastMessageReceived);
        }

        [Test]
        public async Task TestClientClosing()
        {
            Assert.NotZero(server.Connected);
            await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).ConfigureAwait(false);
            await Task.Delay(500).ConfigureAwait(false);
            Assert.IsTrue(server.RemoteInitiatedDisconnect);
            Assert.Zero(server.Connected);
        }

        [Test]
        public async Task TestServerClosing()
        {
            Assert.NotZero(server.Connected);
            await server.Close().ConfigureAwait(false);
            Assert.IsFalse(server.RemoteInitiatedDisconnect);
            Assert.Zero(server.Connected);
        }

        private class ServerWebSocket : WebSocketServer
        {
            public override string Endpoint => @"test";
            public bool RemoteInitiatedDisconnect { get; private set; }
            public string LastMessageReceived { get; private set; }

            protected override void OnConnectionMessage(WebSocketConnection connection, Message message)
            {
                LastMessageReceived = Encoding.UTF8.GetString(message.Content.Span);
            }

            protected override void OnConnectionClose(WebSocketConnection connection, bool requested)
            {
                RemoteInitiatedDisconnect = !requested;
            }
        }
    }
}
