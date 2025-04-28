// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerPositionDisplay : CompositeDrawable
    {
        private readonly IBindable<APIUser> user = new Bindable<APIUser>();
        private readonly IBindableList<GameplayLeaderboardScore> scores = new BindableList<GameplayLeaderboardScore>();
        private readonly BindableBool showLeaderboard = new BindableBool();
        private readonly IBindable<LocalUserPlayingState> localUserPlayingState = new Bindable<LocalUserPlayingState>();

        private readonly Bindable<int?> position = new Bindable<int?>();

        private OsuSpriteText positionText = null!;

        [BackgroundDependencyLoader]
        private void load(IGameplayLeaderboardProvider leaderboardProvider, IAPIProvider api, OsuConfigManager configManager, GameplayState gameplayState)
        {
            scores.BindTo(leaderboardProvider.Scores);
            user.BindTo(api.LocalUser);
            configManager.BindWith(OsuSetting.GameplayLeaderboard, showLeaderboard);
            localUserPlayingState.BindTo(gameplayState.PlayingState);

            AutoSizeAxes = Axes.Both;
            InternalChild = positionText = new OsuSpriteText
            {
                Alpha = 0.5f,
                Font = OsuFont.Torus.With(size: 60, weight: FontWeight.Light),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            user.BindValueChanged(_ => updateState());
            scores.BindCollectionChanged((_, __) => updateState());
            showLeaderboard.BindValueChanged(_ => updateState());
            localUserPlayingState.BindValueChanged(_ => updateState(), true);

            position.BindValueChanged(_ => positionText.Text = position.Value != null ? $@"#{position.Value.Value:N0}" : "-", true);
        }

        private void updateState()
        {
            position.UnbindBindings();

            var userScore = scores.SingleOrDefault(s => s.User.Equals(user.Value));
            if (userScore != null)
                position.BindTo(userScore.Position);
            else
                position.Value = null;

            Alpha = userScore != null && (showLeaderboard.Value || localUserPlayingState.Value == LocalUserPlayingState.Break) ? 1 : 0;
        }
    }
}
