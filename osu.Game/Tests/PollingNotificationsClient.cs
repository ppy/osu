// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using System.Threading.Tasks;
using osu.Game.Online.API;
using osu.Game.Online.Notifications;

namespace osu.Game.Tests
{
    /// <summary>
    /// A notifications client which polls for new messages every second.
    /// </summary>
    public class PollingNotificationsClient : NotificationsClient
    {
        public PollingNotificationsClient(IAPIProvider api)
            : base(api)
        {
        }

        public override Task ConnectAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await API.PerformAsync(CreateInitialFetchRequest()).ConfigureAwait(true);
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(true);
                }
            }, cancellationToken);

            return Task.CompletedTask;
        }
    }
}
