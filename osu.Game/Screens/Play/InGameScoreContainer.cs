// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Scoring;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.Play
{
    public class InGameScoreContainer : FillFlowContainer<InGameScoreItem>
    {
        /// <summary>
        /// Called once an item's score has changed.
        /// Useful for doing calculations on what score to show or hide next. (scrolling system)
        /// </summary>
        public Action OnScoreChange;

        /// <summary>
        /// Whether to declare a new position for un-positioned players.
        /// Must be disabled for online leaderboards with top 50 scores only.
        /// </summary>
        public bool DeclareNewPosition = true;

        public InGameScoreContainer()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(2.5f);
            LayoutDuration = 500;
            LayoutEasing = Easing.OutQuint;
        }

        /// <summary>
        /// Adds a real-time player score item whose score is updated via a <see cref="BindableDouble"/>.
        /// </summary>
        /// <param name="currentScore">The bindable current score of the player.</param>
        /// <param name="user">The player user.</param>
        /// <returns>Returns the drawable score item of that player.</returns>
        public InGameScoreItem AddRealTimePlayer(BindableDouble currentScore, User user = null)
        {
            if (currentScore == null)
                return null;

            var scoreItem = addScore(currentScore.Value, user);
            currentScore.ValueChanged += s => scoreItem.TotalScore = s.NewValue;

            return scoreItem;
        }

        /// <summary>
        /// Adds a score item based off a <see cref="ScoreInfo"/> with an initial position.
        /// </summary>
        /// <param name="score">The score info to use for this item.</param>
        /// <param name="position">The initial position of this item.</param>
        /// <returns>Returns the drawable score item of that player.</returns>
        public InGameScoreItem AddScore(ScoreInfo score, int? position = null) => score != null ? addScore(score.TotalScore, score.User, position) : null;

        private int maxPosition => this.Where(i => i.ScorePosition.HasValue).Max(i => i.ScorePosition) ?? 0;

        private InGameScoreItem addScore(double totalScore, User user = null, int? position = null)
        {
            var scoreItem = new InGameScoreItem
            {
                User = user,
                TotalScore = totalScore,
                ScorePosition = position,
                OnScoreChange = updateScores,
            };

            Add(scoreItem);
            SetLayoutPosition(scoreItem, position ?? maxPosition + 1);

            reorderPositions();

            return scoreItem;
        }

        private void reorderPositions()
        {
            var orderedByScore = this.OrderByDescending(i => i.TotalScore).ToList();
            var orderedPositions = this.OrderByDescending(i => i.ScorePosition.HasValue).ThenBy(i => i.ScorePosition).Select(i => i.ScorePosition).ToList();

            var newDeclaredPosition = maxPosition + 1;

            for (int i = 0; i < Count; i++)
            {
                int newPosition = orderedPositions[i] ?? newDeclaredPosition;

                SetLayoutPosition(orderedByScore[i], newPosition);
                orderedByScore[i].ScorePosition = DeclareNewPosition ? newPosition : orderedPositions[i];
            }
        }

        private void updateScores()
        {
            reorderPositions();

            OnScoreChange?.Invoke();
        }
    }

    public class InGameScoreItem : CompositeDrawable
    {
        private readonly OsuSpriteText positionText, positionSymbol, userString;
        private readonly GlowingSpriteText scoreText;

        public Action OnScoreChange;

        private int? scorePosition;

        public int? ScorePosition
        {
            get => scorePosition;
            set
            {
                scorePosition = value;

                if (scorePosition.HasValue)
                    positionText.Text = $"#{scorePosition.Value.ToMetric(decimals: scorePosition < 100000 ? 1 : 0)}";

                positionText.FadeTo(scorePosition.HasValue ? 1 : 0, 100);
                positionSymbol.FadeTo(scorePosition.HasValue ? 1 : 0, 100);
            }
        }

        private double totalScore;

        public double TotalScore
        {
            get => totalScore;
            set
            {
                totalScore = value;
                scoreText.Text = totalScore.ToString("N0");

                OnScoreChange?.Invoke();
            }
        }

        private User user;

        public User User
        {
            get => user;
            set
            {
                user = value;
                userString.Text = user?.Username;
            }
        }

        public InGameScoreItem()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new Container
            {
                Masking = true,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopRight,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Margin = new MarginPadding { Right = 2.5f },
                        Spacing = new Vector2(2.5f),
                        Children = new[]
                        {
                            positionText = new OsuSpriteText
                            {
                                Alpha = 0,
                                Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                            },
                            positionSymbol = new OsuSpriteText
                            {
                                Alpha = 0,
                                Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                                Text = ">",
                            },
                        }
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopLeft,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Margin = new MarginPadding { Left = 2.5f },
                        Spacing = new Vector2(2.5f),
                        Children = new Drawable[]
                        {
                            userString = new OsuSpriteText
                            {
                                Size = new Vector2(80, 16),
                                Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true),
                            },
                            scoreText = new GlowingSpriteText
                            {
                                GlowColour = OsuColour.FromHex(@"83ccfa"),
                                Font = OsuFont.Numeric.With(size: 14),
                            }
                        }
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            positionText.Colour = colours.YellowLight;
            positionSymbol.Colour = colours.Yellow;
        }
    }
}
