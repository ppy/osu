// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using System.Threading.Tasks;
using osu.Game.Online.API;
using osu.Game.Online.Notifications;

namespace osu.Game.Tests
{
    /// <summary>
    /// A connector for <see cref="PollingNotificationsClient"/>s that poll for new messages.
    /// </summary>
    public class PollingNotificationsClientConnector : NotificationsClientConnector
    {
        public PollingNotificationsClientConnector(IAPIProvider api)
            : base(api)
        {
        }

        protected override Task<NotificationsClient> BuildNotificationClientAsync(CancellationToken cancellationToken)
            => Task.FromResult((NotificationsClient)new PollingNotificationsClient(API));
    }
}
