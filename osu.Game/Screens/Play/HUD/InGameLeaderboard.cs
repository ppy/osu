// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class InGameLeaderboard : CompositeDrawable
    {
        protected readonly FillFlowContainer<InGameScoreItem> ScoresContainer;

        protected readonly InGameScoreItem UserScoreItem;
        public readonly BindableDouble UserTotalScore = new BindableDouble();

        private InGameScoreItem userBestScoreItem;

        private const int default_maximum_scores = number_of_scores_to_show * 2 + 1;
        private int maximumScores => default_maximum_scores + (userBestScoreItem != null ? 1 : 0);

        public User PlayerUser
        {
            get => UserScoreItem.User;
            set => UserScoreItem.User = value;
        }

        private List<ScoreInfo> leaderboardScores;

        private ILeaderboard leaderboard;

        public ILeaderboard Leaderboard
        {
            get => leaderboard;
            set
            {
                leaderboard = value;

                if (leaderboard == null)
                    return;

                leaderboardScores = leaderboard.Scores?.OrderByDescending(s => s.TotalScore).ToList();

                if (leaderboardScores == null || !leaderboardScores.Any())
                    return;

                addLeaderboardScores();

                if (leaderboard is BeatmapLeaderboard beatmapLeaderboard)
                    if (leaderboardScores?.TrueForAll(s => s.User.Username != beatmapLeaderboard.TopScore.Value?.Score.User.Username) ?? false)
                        userBestScoreItem = addScore(beatmapLeaderboard.TopScore.Value?.Score, beatmapLeaderboard.TopScore.Value?.Position);

                ScoresContainer.Add(UserScoreItem);

                // set user score item position below everything at creation
                ScoresContainer.SetLayoutPosition(UserScoreItem, ScoresContainer.Where(i => i.ScorePosition.HasValue).Max(i => i.ScorePosition.Value) + 1);
            }
        }

        public InGameLeaderboard()
        {
            AutoSizeAxes = Axes.Y;

            InternalChild = ScoresContainer = new FillFlowContainer<InGameScoreItem>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                LayoutDuration = 500,
                LayoutEasing = Easing.OutQuint,
                Spacing = new Vector2(2.5f),
            };

            UserScoreItem = new InGameScoreItem { OnScoreChange = updatePositions };
            UserTotalScore.ValueChanged += s => UserScoreItem.TotalScore = (long)s.NewValue;
        }

        private InGameScoreItem addScore(ScoreInfo score, int? position)
        {
            if (score == null)
                return null;

            var scoreItem = new InGameScoreItem
            {
                User = score.User,
                TotalScore = score.TotalScore,
                ScorePosition = position,
                OnScoreChange = updatePositions,
            };

            ScoresContainer.Add(scoreItem);
            ScoresContainer.SetLayoutPosition(scoreItem, position ?? ScoresContainer.Where(i => i.ScorePosition.HasValue).Max(i => i.ScorePosition.Value) + 1);

            reorderScores();

            return scoreItem;
        }

        /// <summary>
        /// Number of score entries to show on the top and the bottom
        /// </summary>
        private const int number_of_scores_to_show = 3;

        private void addLeaderboardScores()
        {
            int count = leaderboardScores.Count;

            for (int i = 0; i < count; i++)
            {
                // must be disabled in real-time leaderboards (gaps not allowed)
                if (i >= number_of_scores_to_show && i < count - number_of_scores_to_show)
                    i = count - number_of_scores_to_show;

                addScore(leaderboardScores[i], i + 1);
            }
        }

        private void reorderScores()
        {
            var orderedByScore = ScoresContainer.OrderByDescending(i => i.TotalScore).ToList();
            var orderedPositions = ScoresContainer.OrderByDescending(i => i.ScorePosition.HasValue).ThenBy(i => i.ScorePosition).Select(i => i.ScorePosition).ToList();

            for (int i = 0; i < orderedByScore.Count; i++)
            {
                int newPosition = orderedPositions[i] ?? orderedPositions.Where(p => p.HasValue).Max(p => p.Value) + 1;

                ScoresContainer.SetLayoutPosition(orderedByScore[i], newPosition);
                orderedByScore[i].ScorePosition = (leaderboard.IsOnlineScope && leaderboardScores.Count == 50) ? orderedPositions[i] : newPosition;
            }
        }

        private void checkForGaps()
        {
            var orderedScores = ScoresContainer.OrderByDescending(i => i.ScorePosition.HasValue).ThenBy(i => i.ScorePosition).ToList();

            if (orderedScores.Last() != UserScoreItem)
            {
                for (int i = 0; i < orderedScores.Count - 1; i++)
                {
                    if (orderedScores[i] == UserScoreItem)
                        continue;

                    int? topPosition = orderedScores[i].ScorePosition, bottomPosition = orderedScores[i + 1].ScorePosition;

                    if (!topPosition.HasValue || !bottomPosition.HasValue)
                        continue;

                    if (topPosition.Value != bottomPosition.Value - 1)
                    {
                        if (bottomPosition.Value >= leaderboardScores.Count)
                            continue;

                        ScoresContainer.RemoveRange(orderedScores.Skip(maximumScores - 1));
                        addScore(leaderboardScores[bottomPosition.Value - 2], bottomPosition.Value - 1);
                    }
                }
            }
        }

        private void updatePositions()
        {
            if (leaderboardScores == null || !leaderboardScores.Any())
                return;

            reorderScores();

            // must be disabled in real-time leaderboards (gaps not allowed)
            checkForGaps();
        }
    }

    public class InGameScoreItem : CompositeDrawable
    {
        private readonly OsuSpriteText positionText, positionPunctation, userText;
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
                    positionText.Text = $"#{scorePosition}";

                positionText.FadeTo(scorePosition.HasValue ? 1 : 0, 100);
                positionPunctation.FadeTo(scorePosition.HasValue ? 1 : 0, 100);
            }
        }

        private long totalScore;

        public long TotalScore
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
                userText.Text = user.Username;
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
                            positionPunctation = new OsuSpriteText
                            {
                                Alpha = 0,
                                Text = ">",
                                Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
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
                            userText = new OsuSpriteText
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
            positionPunctation.Colour = colours.Yellow;
        }
    }
}
