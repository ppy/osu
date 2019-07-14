// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;

namespace osu.Game.Screens.Select.Details
{
    public class UserTopScoreContainer : VisibilityContainer
    {
        private const int height = 150;
        private const int duration = 300;

        private readonly Container contentContainer;
        private readonly Container scoreContainer;

        public Bindable<APILegacyUserTopScoreInfo> TopScore = new Bindable<APILegacyUserTopScoreInfo>();

        protected override bool StartHidden => true;

        public UserTopScoreContainer()
        {
            RelativeSizeAxes = Axes.X;
            Height = height;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            Children = new Drawable[]
            {
                contentContainer = new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Height = height,
                    RelativeSizeAxes = Axes.X,
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
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        }
                    }
                }
            };

            TopScore.BindValueChanged((score) => onScoreChanged(score.NewValue));
        }

        private void onScoreChanged(APILegacyUserTopScoreInfo score)
        {
            scoreContainer.Clear();

            if (score != null)
                scoreContainer.Add(new LeaderboardScore(score.Score, score.Position));
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
