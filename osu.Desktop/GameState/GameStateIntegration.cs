// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Microsoft.AspNetCore.SignalR;
using osu.Desktop.GameState.Handlers;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Configuration;

namespace osu.Desktop.GameState
{
    public partial class GameStateIntegration : Container
    {
        private Bindable<bool> integrationEnabled = null!;
        private Bindable<string?> serverUrl = null!;

        private ScheduledDelegate? toggleServerDelegate;

        private GameStateServer server = null!;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, SessionStatics session)
        {
            integrationEnabled = config.GetBindable<bool>(OsuSetting.GameStateIntegration);
            serverUrl = session.GetBindable<string?>(Static.GameStateServerUrl);

            server = new GameStateServer();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            integrationEnabled.ValueChanged += onEnabledChanged;

            server.OnReady += onReady;
            server.OnStopped += onStopped;

            if (integrationEnabled.Value)
                server.Start();
        }

        private void onEnabledChanged(ValueChangedEvent<bool> e)
        {
            toggleServerDelegate?.Cancel();

            toggleServerDelegate = Scheduler.AddDelayed(() =>
            {
                if (e.NewValue)
                    server.Start();
                else
                    server.Stop();
            }, 1000);
        }

        private void onReady(string serverUrl, IHubContext<GameStateHub, IGameStateClient> hubContext)
        {
            this.serverUrl.Value = serverUrl;

            Scheduler.Add(() => Add(new GameplayStateHandler(hubContext)));
        }

        private void onStopped()
        {
            Scheduler.Add(Clear);
            serverUrl.Value = null;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (!server.IsRunning)
                server.Stop();
        }
    }
}
