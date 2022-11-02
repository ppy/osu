// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace osu.Game.Online
{
    public class HubClient : PersistentEndpointClient
    {
        public readonly HubConnection Connection;

        public HubClient(HubConnection connection)
        {
            Connection = connection;
            Connection.Closed += InvokeClosed;
        }

        public override Task ConnectAsync(CancellationToken cancellationToken) => Connection.StartAsync(cancellationToken);

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync().ConfigureAwait(false);
            await Connection.DisposeAsync().ConfigureAwait(false);
        }
    }
}
