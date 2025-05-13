// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select.Leaderboards;
using osuTK;
using osuTK.Graphics;

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

        private Drawable localPlayerMarker = null!;

        private const float marker_size = 5;
        private const float width = 90;

        private const float min_alpha = 0.2f;
        private const float max_alpha = 0.4f;

        [BackgroundDependencyLoader]
        private void load(IGameplayLeaderboardProvider leaderboardProvider, IAPIProvider api, OsuConfigManager configManager, GameplayState gameplayState)
        {
            scores.BindTo(leaderboardProvider.Scores);
            user.BindTo(api.LocalUser);
            configManager.BindWith(OsuSetting.GameplayLeaderboard, showLeaderboard);
            localUserPlayingState.BindTo(gameplayState.PlayingState);

            AutoSizeAxes = Axes.Y;
            Width = width;

            InternalChildren = new Drawable[]
            {
                positionText = new OsuSpriteText
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Alpha = 0,
                    Padding = new MarginPadding { Right = -5 },
                    Font = OsuFont.Torus.With(size: 60, weight: FontWeight.Light, fixedWidth: true),
                    Spacing = new Vector2(-8, 0),
                },
                new Container
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    RelativeSizeAxes = Axes.X,
                    Height = marker_size - 2,
                    Children = new[]
                    {
                        new Circle
                        {
                            Colour = ColourInfo.GradientHorizontal(
                                Color4.White.Opacity(max_alpha),
                                Color4.White.Opacity(min_alpha)
                            ),
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                        },
                        localPlayerMarker = new Circle
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Colour = Color4.Cyan,
                            Size = new Vector2(marker_size),
                            Blending = BlendingParameters.Additive,
                            Alpha = 0.4f,
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            user.BindValueChanged(_ => updateState());
            scores.BindCollectionChanged((_, __) => updateState());
            showLeaderboard.BindValueChanged(_ => updateState());
            localUserPlayingState.BindValueChanged(_ => updateState(), true);

            position.BindValueChanged(_ =>
            {
                if (position.Value == null)
                {
                    positionText.Alpha = 0;
                    positionText.Text = "-";
                    localPlayerMarker.FadeOut();
                    return;
                }

                float relativePosition = (float)(position.Value.Value - 1) / scores.Count;

                positionText.Text = $@"#{position.Value.Value:N0}";
                positionText.Alpha = min_alpha + (max_alpha - min_alpha) * (1 - relativePosition);

                localPlayerMarker.FadeIn();
                localPlayerMarker.MoveToX(Math.Min(relativePosition * width, width - marker_size), 1000, Easing.OutQuint);
            }, true);
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
