// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Microsoft.AspNetCore.SignalR;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Desktop.GameState.Handlers
{
    public partial class GameplayStateHandler : Component
    {
        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]

        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        private readonly IHubContext<GameStateHub, IGameStateClient> hubContext;

        private IBindable<UserActivity?> userActivity = null!;

        public GameplayStateHandler(IHubContext<GameStateHub, IGameStateClient> hubContext)
        {
            this.hubContext = hubContext;
        }

        [BackgroundDependencyLoader]
        private void load(SessionStatics session)
        {
            userActivity = session.GetBindable<UserActivity?>(Static.UserOnlineActivity);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userActivity.ValueChanged += onUserActivityChanged;
        }

        private void onUserActivityChanged(ValueChangedEvent<UserActivity?> e)
        {
            if (e.NewValue is UserActivity.InSoloGame)
            {
                hubContext.Clients.All.SoloGameplayStarted(ruleset.Value.OnlineID, $"{beatmap.Value.BeatmapInfo.GetDisplayTitle()}").FireAndForget();
            }
            else if (e.OldValue is UserActivity.InSoloGame)
            {
                hubContext.Clients.All.SoloGameplayEnded().FireAndForget();
            }
        }
    }
}
