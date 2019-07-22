// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using System;

namespace osu.Game.Screens.Select.Details
{
    public class UserTopScoreContainer : VisibilityContainer
    {
        private const int height = 90;
        private const int duration = 300;

        private readonly Container contentContainer;
        private readonly Container scoreContainer;

        public Bindable<APILegacyUserTopScoreInfo> Score = new Bindable<APILegacyUserTopScoreInfo>();

        public Action<ScoreInfo> ScoreSelected;

        protected override bool StartHidden => true;

        public UserTopScoreContainer()
        {
            RelativeSizeAxes = Axes.X;
            Height = height;

            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            Children = new Drawable[]
            {
                contentContainer = new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Margin = new MarginPadding { Top = 5 },
                            Text = @"your personal best".ToUpper(),
                            Font = OsuFont.GetFont(size: 15, weight: FontWeight.Bold),
                        },
                        scoreContainer = new Container
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        }
                    }
                }
            };

            Score.BindValueChanged(onScoreChanged);
        }

        private void onScoreChanged(ValueChangedEvent<APILegacyUserTopScoreInfo> score)
        {
            var newScore = score.NewValue;

            if (newScore == null)
            {
                Hide();
                return;
            }

            scoreContainer.Child = new LeaderboardScore(newScore.Score, newScore.Position)
            {
                Action = () => ScoreSelected?.Invoke(newScore.Score)
            };

            Show();
        }

        protected override void PopIn()
        {
            this.ResizeHeightTo(height, duration, Easing.OutQuint);
            contentContainer.FadeIn(duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.ResizeHeightTo(0, duration, Easing.OutQuint);
            contentContainer.FadeOut(duration, Easing.OutQuint);
        }
    }
}
