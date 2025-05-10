// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Online.WebSockets;

namespace osu.Game.Online.Broadcasts
{

    public partial class BroadcastServer : Component, IBroadcastServer
    {
        private readonly Uri uri;
        private readonly WebSocketServer server = new WebSocketServer();

        private Bindable<bool>? broadcast;

        public BroadcastServer(Uri uri)
        {
            this.uri = uri;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            broadcast = config.GetBindable<bool>(OsuSetting.BroadcastGameState);
            broadcast.BindValueChanged(value =>
            {
                if (value.NewValue)
                {
                    server.Start(uri);
                }
                else
                {
                    broadcast.Disabled = true;
                    server.StopAsync().ContinueWith(_ => broadcast.Disabled = false);
                }
            }, true);
        }

        async Task IBroadcastServer.Broadcast(string message)
        {
            await server.SendAsync(message).ConfigureAwait(false);
        }
    }
}
