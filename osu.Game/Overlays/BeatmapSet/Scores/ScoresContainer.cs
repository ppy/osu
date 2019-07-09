// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoresContainer : CompositeDrawable
    {
        private const int spacing = 15;
        private const int padding = 20;

        private readonly Box background;
        private readonly ScoreTable scoreTable;

        private readonly FillFlowContainer topScoresContainer;
        private readonly ContentContainer resizableContainer;
        private readonly LoadingContainer loadingContainer;

        public ScoresContainer()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        loadingContainer = new LoadingContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Masking = true,
                        },
                        resizableContainer = new ContentContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Width = 0.95f,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, spacing),
                                    Padding = new MarginPadding { Vertical = padding },
                                    Children = new Drawable[]
                                    {
                                        topScoresContainer = new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(0, 5),
                                        },
                                        scoreTable = new ScoreTable
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                        }
                                    }
                                },
                            }
                        },
                    }
                }
            };

            Loading = true;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Gray2;
        }

        private bool loading;

        public bool Loading
        {
            get => loading;
            set
            {
                if (loading == value)
                    return;

                loading = value;

                if (value)
                {
                    loadingContainer.Show();
                    resizableContainer.Hide();
                }
                else
                {
                    loadingContainer.Hide();
                    resizableContainer.Show();
                }
            }
        }

        private APILegacyScores scores;

        public APILegacyScores Scores
        {
            get => scores;
            set
            {
                scores = value;

                updateDisplay();
            }
        }

        private void updateDisplay()
        {
            topScoresContainer.Clear();

            scoreTable.Scores = scores?.Scores.Count > 1 ? scores.Scores : new List<APILegacyScoreInfo>();
            scoreTable.FadeTo(scores?.Scores.Count > 1 ? 1 : 0);

            if (scores?.Scores.Any() == true)
            {
                topScoresContainer.Add(new DrawableTopScore(scores.Scores.FirstOrDefault()));

                var userScore = scores.UserScore;

                if (userScore != null && userScore.Position != 1)
                    topScoresContainer.Add(new DrawableTopScore(userScore.Score, userScore.Position));
            }

            Loading = false;
        }

        private class ContentContainer : VisibilityContainer
        {
            private const int duration = 300;

            private float maxHeight;

            protected override void PopIn() => this.ResizeHeightTo(maxHeight, duration, Easing.OutQuint);

            protected override void PopOut() => this.ResizeHeightTo(0, duration, Easing.OutQuint);

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                if (State.Value == Visibility.Hidden)
                    return;

                float height = 0;

                foreach (var c in Children)
                {
                    height += c.Height;
                }

                maxHeight = height;

                this.ResizeHeightTo(maxHeight, duration, Easing.OutQuint);
            }
        }

        private class LoadingContainer : VisibilityContainer
        {
            private const int duration = 300;
            private const int height = 50;

            private readonly LoadingAnimation loadingAnimation;

            public LoadingContainer()
            {
                Child = loadingAnimation = new LoadingAnimation();
            }

            protected override void PopIn()
            {
                this.ResizeHeightTo(height, duration, Easing.OutQuint);
                loadingAnimation.Show();
            }

            protected override void PopOut()
            {
                this.ResizeHeightTo(0, duration, Easing.OutQuint);
                loadingAnimation.Hide();
            }
        }
    }
}
