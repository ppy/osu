// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net.WebSockets;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Game.Online.Broadcasts;

namespace osu.Game.Tests.Online
{
    [TestFixture]
    public class TestSceneBroadcaster
    {
        private ClientWebSocket client;
        private Broadcaster server;

        [SetUp]
        public void SetUp()
        {
            client = new ClientWebSocket();
            server = new Broadcaster();

            server.Start();
            client.ConnectAsync(new Uri("ws://localhost:7270/"), CancellationToken.None).WaitSafely();
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
        public void TestBroadcast()
        {
            var received = new ArraySegment<byte>(new byte[4 * 4096]);
            client.ReceiveAsync(received, CancellationToken.None);
            server.Broadcast("Hello World");
            Thread.Sleep(500);
            Assert.IsTrue(received.Count > 0);
        }

        [Test]
        public void TestClientClosing()
        {
            Assert.IsTrue(server.Connected > 0);
            client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).WaitSafely();
            Thread.Sleep(500);
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
    }
}
