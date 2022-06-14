// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Online.Leaderboards
{
    public class UserTopScoreContainer<TScoreInfo> : VisibilityContainer
    {
        private const int duration = 500;

        public Bindable<TScoreInfo> Score = new Bindable<TScoreInfo>();

        private readonly Container scoreContainer;
        private readonly Func<TScoreInfo, LeaderboardScore> createScoreDelegate;

        protected override bool StartHidden => true;

        public UserTopScoreContainer(Func<TScoreInfo, LeaderboardScore> createScoreDelegate)
        {
            this.createScoreDelegate = createScoreDelegate;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Margin = new MarginPadding { Vertical = 5 };

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = @"your personal best".ToUpper(),
                            Font = OsuFont.GetFont(size: 15, weight: FontWeight.Bold),
                        },
                        scoreContainer = new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        }
                    }
                }
            };

            Score.BindValueChanged(onScoreChanged);
        }

        private CancellationTokenSource loadScoreCancellation;

        private void onScoreChanged(ValueChangedEvent<TScoreInfo> score)
        {
            var newScore = score.NewValue;

            scoreContainer.Clear();
            loadScoreCancellation?.Cancel();

            if (newScore == null)
                return;

            LoadComponentAsync(createScoreDelegate(newScore), drawableScore =>
            {
                scoreContainer.Child = drawableScore;
                drawableScore.FadeInFromZero(duration, Easing.OutQuint);
            }, (loadScoreCancellation = new CancellationTokenSource()).Token);
        }

        protected override void PopIn() => this.FadeIn(duration, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(duration, Easing.OutQuint);
    }
}
