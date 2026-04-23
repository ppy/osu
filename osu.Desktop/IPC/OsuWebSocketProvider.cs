// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using osu.Desktop.IPC.Messages;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.IPC;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace osu.Desktop.IPC
{
    public partial class OsuWebSocketProvider : Component
    {
        private WebSocketServer? server;
        private readonly Bindable<ScoreInfo> lastLocalScore = new Bindable<ScoreInfo>();

        [BackgroundDependencyLoader]
        private void load(SessionStatics sessionStatics)
        {
            server = new WebSocketServer(49727);
            server.StartAsync().FireAndForget(onError: ex => Logger.Error(ex, "Failed to start websocket"));

            sessionStatics.BindWith(Static.LastLocalUserScore, lastLocalScore);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            lastLocalScore.BindValueChanged(val =>
            {
                if (val.NewValue == null)
                    return;

                if (server?.IsRunning != true)
                    return;

                var msg = new HitCountMessage { NewHits = val.NewValue.Statistics.Where(kv => kv.Key.IsBasic() && kv.Key.IsHit()).Sum(kv => kv.Value) };
                broadcast(msg);
            });
        }

        private void broadcast(OsuWebSocketMessage message)
        {
            if (server?.IsRunning != true)
                return;

            string messageString = JsonConvert.SerializeObject(message);
            server.BroadcastAsync(messageString).FireAndForget();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (server?.IsRunning == true)
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(10));
                server.StopAsync(cts.Token).WaitSafely();
                server = null;
            }
        }
    }
}
