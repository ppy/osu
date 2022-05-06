// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Game.Online.WebSockets;

namespace osu.Game.Tests.Online
{
    [TestFixture]
    public class TestSceneWebSocketClient
    {
        private ClientWebSocket client;
        private TestWebSocketClient server;

        [SetUp]
        public void SetUp()
        {
            client = new ClientWebSocket();
            server = new TestWebSocketClient();

            server.Start();
            client.ConnectAsync(new Uri("ws://localhost:7270/test/"), CancellationToken.None).WaitSafely();
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
            var received = new ArraySegment<byte>(new byte[4096]);
            client.ReceiveAsync(received, CancellationToken.None);
            server.Broadcast("Hello World");
            Thread.Sleep(1000);
            Assert.IsTrue(received.Count > 0);
        }

        [Test]
        public void TestClientMessage()
        {
            var sent = new ArraySegment<byte>(Encoding.UTF8.GetBytes("Hello World"));
            client.SendAsync(sent, WebSocketMessageType.Text, true, CancellationToken.None).WaitSafely();
            Thread.Sleep(1000);
            Assert.AreEqual("Hello World", server.LastMessageReceived);
        }

        [Test]
        public void TestClientClosing()
        {
            Assert.IsTrue(server.Connected > 0);
            client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).WaitSafely();
            Thread.Sleep(1000);
            Assert.IsTrue(server.Connected == 0);
        }

        [Test]
        public void TestServerClosing()
        {
            Assert.IsTrue(server.Connected > 0);
            server.Close();
            Thread.Sleep(1000);
            Assert.IsTrue(server.Connected == 0);
        }

        private class TestWebSocketClient : WebSocketClient
        {
            public override string Endpoint => @"test";
            public string LastMessageReceived { get; private set; }

            protected override void OnConnectionMessage(WebSocketConnection connection, Message message)
            {
                LastMessageReceived = Encoding.UTF8.GetString(message.Content.Span);
            }
        }
    }
}
