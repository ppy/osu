// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;

namespace osu.Game.Online.WebSockets
{
    public class GameStateBroadcastServer : WebSocketServer
    {
        public override string Endpoint => @"state";

        private Bindable<bool> enabled;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            enabled = config.GetBindable<bool>(OsuSetting.BroadcastGameState);
            enabled.BindValueChanged(e =>
            {
                if (e.NewValue)
                {
                    Start();
                }
                else
                {
                    enabled.Disabled = true;
                    Task.Run(() => Close()).ContinueWith(t => enabled.Disabled = false);
                }
            }, true);
        }

        public void Add(GameStateBroadcaster broadcaster)
            => AddInternal(broadcaster);

        public void AddRange(IEnumerable<GameStateBroadcaster> broadcasters)
            => AddRangeInternal(broadcasters);

        public void Remove(GameStateBroadcaster broadcaster)
            => RemoveInternal(broadcaster);

        protected override void OnConnectionReady(WebSocketConnection connection)
        {
            var broadcasters = InternalChildren.OfType<GameStateBroadcaster>();

            foreach (var broadcaster in broadcasters)
                broadcaster.Broadcast();
        }
    }
}
