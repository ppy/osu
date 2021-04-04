// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net.WebSockets;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Online;

namespace osu.Game.Tests.Visual.Components
{
    public class TestSceneGameStateBroadcaster : OsuTestScene
    {
        private readonly ClientWebSocket client = new ClientWebSocket();

        private readonly GameStateBroadcaster broadcaster;

        private readonly Bindable<bool> enabled = new Bindable<bool>();

        private ArraySegment<byte> message;

        public TestSceneGameStateBroadcaster()
        {
            Add(broadcaster = new GameStateBroadcaster());
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.PublishGameState, enabled);
        }

        [Test]
        public void TestBroadcasterAlive()
        {
            AddStep("enable", () => enabled.Value = true);
            AddUntilStep("wait till enabled", () => broadcaster.IsListening);
            AddStep("disable", () => enabled.Value = false);
            AddUntilStep("wait till disabled", () => !broadcaster.IsListening);
        }

        [Test]
        public void TestClientConnection()
        {
            AddStep("enable", () => enabled.Value = true);
            AddUntilStep("wait till enabled", () => broadcaster.IsListening);

            AddStep("connect", () => client.ConnectAsync(new Uri("ws://localhost:7270"), CancellationToken.None));
            AddUntilStep("wait for connection", () => client.State == WebSocketState.Open);
            AddAssert("has connected client", () => broadcaster.Clients.Count > 0);

            AddStep("listen", () => client.ReceiveAsync(message = new ArraySegment<byte>(), CancellationToken.None));
            AddStep("broadcast", () => broadcaster.Broadcast());
            AddWaitStep("wait", 20);
            AddAssert("confirm data received", () => message.Count > 0);

            AddStep("disconnect", () => client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None));
            AddUntilStep("wait for disconnection", () => client.State == WebSocketState.Closed);
            AddAssert("has no connected client", () => broadcaster.Clients.Count < 1);
        }
    }
}
