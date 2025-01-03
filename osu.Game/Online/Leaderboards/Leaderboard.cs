// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Placeholders;
using osuTK;
using osuTK.Graphics;
using osu.Game.Localisation;
using System.Threading;

namespace osu.Game.Online.Leaderboards
{
    /// <summary>
    /// A leaderboard which displays a scrolling list of top scores, along with a single "user best"
    /// for the local user.
    /// </summary>
    /// <typeparam name="TScope">The scope of the leaderboard (ie. global or local).</typeparam>
    /// <typeparam name="TScoreInfo">The score model class.</typeparam>
    public abstract partial class Leaderboard<TScope, TScoreInfo> : CompositeDrawable
    {
        protected LeaderboardScoresProvider<TScope, TScoreInfo> LeaderboardScoresProvider;

        private const double fade_duration = 300;

        private readonly OsuScrollContainer scrollContainer;
        private readonly Container placeholderContainer;
        private readonly UserTopScoreContainer<TScoreInfo> userScoreContainer;

        private FillFlowContainer<LeaderboardScore>? scoreFlowContainer;

        private readonly LoadingSpinner loading;

        private CancellationTokenSource? currentScoresAsyncLoadCancellationSource;

        protected Leaderboard(LeaderboardScoresProvider<TScope, TScoreInfo> leaderboardScoresProvider)
        {
            LeaderboardScoresProvider = leaderboardScoresProvider;

            InternalChildren = new Drawable[]
            {
                new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(),
                            new Dimension(GridSizeMode.AutoSize),
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                scrollContainer = new OsuScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ScrollbarVisible = false,
                                }
                            },
                            new Drawable[]
                            {
                                userScoreContainer = new UserTopScoreContainer<TScoreInfo>(CreateDrawableTopScore)
                            },
                        },
                    },
                },
                loading = new LoadingSpinner(),
                placeholderContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LeaderboardScoresProvider.State.BindValueChanged(state => onStateChange(state.NewValue));
        }

        private void onStateChange(LeaderboardState state)
        {
            Schedule(applyNewScores);

            void applyNewScores()
            {
                userScoreContainer.Score.Value = LeaderboardScoresProvider.UserScore;

                if (LeaderboardScoresProvider.UserScore == null)
                    userScoreContainer.Hide();
                else
                    userScoreContainer.Show();

                updateScoresDrawables(state);
            }
        }

        protected abstract LeaderboardScore CreateDrawableScore(TScoreInfo model, int index);

        protected abstract LeaderboardScore CreateDrawableTopScore(TScoreInfo model);

        private void updateScoresDrawables(LeaderboardState state)
        {
            currentScoresAsyncLoadCancellationSource?.Cancel();

            scoreFlowContainer?
                .FadeOut(fade_duration, Easing.OutQuint)
                .Expire();
            scoreFlowContainer = null;

            if (!LeaderboardScoresProvider.Scores.Any())
            {
                setPlaceholder(state);
                return;
            }

            LoadComponentAsync(new FillFlowContainer<LeaderboardScore>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0f, 5f),
                Padding = new MarginPadding { Top = 10, Bottom = 5 },
                ChildrenEnumerable = LeaderboardScoresProvider.Scores.Select((s, index) => CreateDrawableScore(s, index + 1))
            }, newFlow =>
            {
                setPlaceholder(state);

                scrollContainer.Add(scoreFlowContainer = newFlow);

                double delay = 0;

                foreach (var s in scoreFlowContainer)
                {
                    using (s.BeginDelayedSequence(delay))
                        s.Show();

                    delay += 50;
                }

                scrollContainer.ScrollToStart(false);
            }, (currentScoresAsyncLoadCancellationSource = new CancellationTokenSource()).Token);
        }

        #region Placeholder handling

        private Placeholder? placeholder;

        private void setPlaceholder(LeaderboardState state)
        {
            if (state == LeaderboardState.Retrieving)
                loading.Show();
            else
                loading.Hide();

            placeholder?.FadeOut(150, Easing.OutQuint).Expire();

            placeholder = getPlaceholderFor(state);

            if (placeholder == null)
                return;

            placeholderContainer.Child = placeholder;

            placeholder.ScaleTo(0.8f).Then().ScaleTo(1, fade_duration * 3, Easing.OutQuint);
            placeholder.FadeInFromZero(fade_duration, Easing.OutQuint);
        }

        private Placeholder? getPlaceholderFor(LeaderboardState state)
        {
            switch (state)
            {
                case LeaderboardState.NetworkFailure:
                    return new ClickablePlaceholder(LeaderboardStrings.CouldntFetchScores, FontAwesome.Solid.Sync)
                    {
                        Action = LeaderboardScoresProvider.RefetchScores
                    };

                case LeaderboardState.NoneSelected:
                    return new MessagePlaceholder(LeaderboardStrings.PleaseSelectABeatmap);

                case LeaderboardState.RulesetUnavailable:
                    return new MessagePlaceholder(LeaderboardStrings.LeaderboardsAreNotAvailableForThisRuleset);

                case LeaderboardState.BeatmapUnavailable:
                    return new MessagePlaceholder(LeaderboardStrings.LeaderboardsAreNotAvailableForThisBeatmap);

                case LeaderboardState.NoScores:
                    return new MessagePlaceholder(LeaderboardStrings.NoRecordsYet);

                case LeaderboardState.NotLoggedIn:
                    return new LoginPlaceholder(LeaderboardStrings.PleaseSignInToViewOnlineLeaderboards);

                case LeaderboardState.NotSupporter:
                    return new MessagePlaceholder(LeaderboardStrings.PleaseInvestInAnOsuSupporterTagToViewThisLeaderboard);

                case LeaderboardState.Retrieving:
                    return null;

                case LeaderboardState.Success:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }

        #endregion

        #region Fade handling

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            float fadeBottom = scrollContainer.Current + scrollContainer.DrawHeight;
            float fadeTop = scrollContainer.Current + LeaderboardScore.HEIGHT;

            if (!scrollContainer.IsScrolledToEnd())
                fadeBottom -= LeaderboardScore.HEIGHT;

            if (scoreFlowContainer == null)
                return;

            foreach (var c in scoreFlowContainer)
            {
                float topY = c.ToSpaceOfOtherDrawable(Vector2.Zero, scoreFlowContainer).Y;
                float bottomY = topY + LeaderboardScore.HEIGHT;

                bool requireBottomFade = bottomY >= fadeBottom;

                if (!requireBottomFade)
                    c.Colour = Color4.White;
                else if (topY > fadeBottom + LeaderboardScore.HEIGHT || bottomY < fadeTop - LeaderboardScore.HEIGHT)
                    c.Colour = Color4.Transparent;
                else
                {
                    if (bottomY - fadeBottom > 0)
                    {
                        c.Colour = ColourInfo.GradientVertical(
                            Color4.White.Opacity(Math.Min(1 - (topY - fadeBottom) / LeaderboardScore.HEIGHT, 1)),
                            Color4.White.Opacity(Math.Min(1 - (bottomY - fadeBottom) / LeaderboardScore.HEIGHT, 1)));
                    }
                    else
                    {
                        c.Colour = ColourInfo.GradientVertical(
                            Color4.White.Opacity(Math.Min(1 - (fadeTop - topY) / LeaderboardScore.HEIGHT, 1)),
                            Color4.White.Opacity(Math.Min(1 - (fadeTop - bottomY) / LeaderboardScore.HEIGHT, 1)));
                    }
                }
            }
        }

        #endregion
    }
}
