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
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select.Leaderboards;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerPositionDisplay : VisibilityContainer
    {
        private readonly IBindable<APIUser> user = new Bindable<APIUser>();
        private readonly IBindableList<GameplayLeaderboardScore> scores = new BindableList<GameplayLeaderboardScore>();
        private readonly BindableBool showLeaderboard = new BindableBool();
        private readonly IBindable<LocalUserPlayingState> localUserPlayingState = new Bindable<LocalUserPlayingState>();

        private readonly Bindable<int?> position = new Bindable<int?>();

        private RollingCounter<int> positionText = null!;

        private Drawable localPlayerMarker = null!;

        private const float marker_size = 5;
        private const float width = 90;

        private const float min_alpha = 0.2f;
        private const float max_alpha = 0.4f;

        private GameplayLeaderboardScore? userScore;

        protected override bool StartHidden => true;

        [BackgroundDependencyLoader]
        private void load(IGameplayLeaderboardProvider leaderboardProvider, IAPIProvider api, OsuConfigManager configManager, GameplayState gameplayState, OsuColour colours)
        {
            scores.BindTo(leaderboardProvider.Scores);
            user.BindTo(api.LocalUser);
            configManager.BindWith(OsuSetting.GameplayLeaderboard, showLeaderboard);
            localUserPlayingState.BindTo(gameplayState.PlayingState);

            AutoSizeAxes = Axes.Y;
            Width = width;

            InternalChildren = new Drawable[]
            {
                positionText = new PositionCounter
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Alpha = 0,
                    Padding = new MarginPadding { Right = -5 },
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
                            Origin = Anchor.Centre,
                            Colour = colours.Blue1,
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

            user.BindValueChanged(_ => updateScoreBindings());
            scores.BindCollectionChanged((_, __) => updateScoreBindings(), true);

            showLeaderboard.BindValueChanged(_ => updateVisibility());
            localUserPlayingState.BindValueChanged(_ => updateVisibility(), true);

            State.BindValueChanged(_ => updatePosition());
            position.BindValueChanged(_ => updatePosition(), true);
        }

        protected override void PopIn()
        {
            this.FadeIn(500, Easing.OutQuint);
            localPlayerMarker.ScaleTo(Vector2.One, 500, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(500, Easing.OutQuint);
            localPlayerMarker.ScaleTo(new Vector2(0.8f), 500, Easing.Out);
        }

        private void updateVisibility()
        {
            bool shouldDisplay = userScore != null && (showLeaderboard.Value || localUserPlayingState.Value == LocalUserPlayingState.Break);

            State.Value = shouldDisplay ? Visibility.Visible : Visibility.Hidden;
        }

        private void updateScoreBindings()
        {
            position.UnbindBindings();

            userScore = scores.SingleOrDefault(s => s.User.Equals(user.Value));
            if (userScore != null)
                position.BindTo(userScore.Position);
            else
                position.Value = null;

            updateVisibility();
            updatePosition();
        }

        private void updatePosition()
        {
            // only update when visible to delay animations.
            if (State.Value != Visibility.Visible) return;

            if (position.Value == null)
            {
                positionText.Alpha = min_alpha;
                positionText.Current.Value = -1;
                localPlayerMarker.FadeOut();
                return;
            }

            float relativePosition = Math.Clamp((float)(position.Value.Value - 1) / Math.Max(scores.Count - 1, 1), 0, 1);

            positionText.Current.Value = position.Value.Value;
            positionText.FadeTo(min_alpha + (max_alpha - min_alpha) * (1 - relativePosition), 1000, Easing.OutPow10);

            localPlayerMarker.FadeIn();
            float markerWidth = Math.Max(marker_size, width / scores.Count);
            localPlayerMarker.ResizeWidthTo(markerWidth, 1000, Easing.OutPow10);
            localPlayerMarker.MoveToX(markerWidth / 2 + (width - markerWidth) * relativePosition, 1000, Easing.OutPow10);
        }

        private partial class PositionCounter : RollingCounter<int>
        {
            protected override double RollingDuration => Current.Value > 0 ? 1000 : 0;
            protected override Easing RollingEasing => Easing.OutPow10;

            protected override LocalisableString FormatCount(int count)
            {
                if (count <= 0)
                    return "-";

                return "#" + base.FormatCount(count);
            }

            protected override OsuSpriteText CreateSpriteText()
            {
                return new OsuSpriteText
                {
                    Font = OsuFont.Torus.With(size: 60, weight: FontWeight.Light, fixedWidth: true),
                    Spacing = new Vector2(-8, 0),
                };
            }
        }
    }
}
