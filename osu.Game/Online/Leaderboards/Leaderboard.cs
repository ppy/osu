// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Placeholders;
using osuTK;
using osuTK.Graphics;
using osu.Game.Localisation;

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
        /// <summary>
        /// The currently displayed scores.
        /// </summary>
        public IBindableList<TScoreInfo> Scores => scores;

        private readonly BindableList<TScoreInfo> scores = new BindableList<TScoreInfo>();

        /// <summary>
        /// Whether the current scope should refetch in response to changes in API connectivity state.
        /// </summary>
        protected abstract bool IsOnlineScope { get; }

        private const double fade_duration = 300;

        private readonly OsuScrollContainer scrollContainer;
        private readonly Container placeholderContainer;
        private readonly UserTopScoreContainer<TScoreInfo> userScoreContainer;

        private FillFlowContainer<LeaderboardScore>? scoreFlowContainer;

        private readonly LoadingSpinner loading;

        private CancellationTokenSource? currentFetchCancellationSource;
        private CancellationTokenSource? currentScoresAsyncLoadCancellationSource;

        private APIRequest? fetchScoresRequest;

        private LeaderboardState state;

        [Resolved(CanBeNull = true)]
        private IAPIProvider? api { get; set; }

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        private TScope scope = default!;

        public TScope Scope
        {
            get => scope;
            set
            {
                if (EqualityComparer<TScope>.Default.Equals(value, scope))
                    return;

                scope = value;
                RefetchScores();
            }
        }

        protected Leaderboard()
        {
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

            if (api != null)
            {
                apiState.BindTo(api.State);
                apiState.BindValueChanged(state =>
                {
                    switch (state.NewValue)
                    {
                        case APIState.Online:
                        case APIState.Offline:
                            if (IsOnlineScope)
                                RefetchScores();

                            break;
                    }
                });
            }

            RefetchScores();
        }

        /// <summary>
        /// Perform a full refetch of scores using current criteria.
        /// </summary>
        public void RefetchScores() => Scheduler.AddOnce(refetchScores);

        /// <summary>
        /// Call when a retrieval or display failure happened to show a relevant message to the user.
        /// </summary>
        /// <param name="state">The state to display.</param>
        protected void SetErrorState(LeaderboardState state)
        {
            switch (state)
            {
                case LeaderboardState.NoScores:
                case LeaderboardState.Retrieving:
                case LeaderboardState.Success:
                    throw new InvalidOperationException($"State {state} cannot be set by a leaderboard implementation.");
            }

            Debug.Assert(!scores.Any());

            setState(state);
        }

        /// <summary>
        /// Call when retrieved scores are ready to be displayed.
        /// </summary>
        /// <param name="scores">The scores to display.</param>
        /// <param name="userScore">The user top score, if any.</param>
        protected void SetScores(IEnumerable<TScoreInfo>? scores, TScoreInfo? userScore = default)
        {
            this.scores.Clear();
            if (scores != null)
                this.scores.AddRange(scores);

            // Non-delayed schedule may potentially run inline (due to IsMainThread check passing) after leaderboard  is disposed.
            // This is guarded against in BeatmapLeaderboard via web request cancellation, but let's be extra safe.
            if (!IsDisposed)
            {
                // Schedule needs to be non-delayed here for the weird logic in refetchScores to work.
                // If it is removed, the placeholder will be incorrectly updated to "no scores" rather than "retrieving".
                // This whole flow should be refactored in the future.
                Scheduler.Add(applyNewScores, false);
            }

            void applyNewScores()
            {
                userScoreContainer.Score.Value = userScore;

                if (userScore == null)
                    userScoreContainer.Hide();
                else
                    userScoreContainer.Show();

                updateScoresDrawables();
            }
        }

        /// <summary>
        /// Performs a fetch/refresh of scores to be displayed.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>An <see cref="APIRequest"/> responsible for the fetch operation. This will be queued and performed automatically.</returns>
        protected abstract APIRequest? FetchScores(CancellationToken cancellationToken);

        protected abstract LeaderboardScore CreateDrawableScore(TScoreInfo model, int index);

        protected abstract LeaderboardScore CreateDrawableTopScore(TScoreInfo model);

        private void refetchScores()
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);

            cancelPendingWork();

            SetScores(null);
            setState(LeaderboardState.Retrieving);

            currentFetchCancellationSource = new CancellationTokenSource();

            fetchScoresRequest = FetchScores(currentFetchCancellationSource.Token);

            if (fetchScoresRequest == null)
                return;

            fetchScoresRequest.Failure += e => Schedule(() =>
            {
                if (e is OperationCanceledException || currentFetchCancellationSource.IsCancellationRequested)
                    return;

                SetErrorState(LeaderboardState.NetworkFailure);
            });

            api?.Queue(fetchScoresRequest);
        }

        private void cancelPendingWork()
        {
            currentFetchCancellationSource?.Cancel();
            currentScoresAsyncLoadCancellationSource?.Cancel();
            fetchScoresRequest?.Cancel();
        }

        private void updateScoresDrawables()
        {
            currentScoresAsyncLoadCancellationSource?.Cancel();

            scoreFlowContainer?
                .FadeOut(fade_duration, Easing.OutQuint)
                .Expire();
            scoreFlowContainer = null;

            if (!scores.Any())
            {
                setState(LeaderboardState.NoScores);
                return;
            }

            LoadComponentAsync(new FillFlowContainer<LeaderboardScore>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0f, 5f),
                Padding = new MarginPadding { Top = 10, Bottom = 5 },
                ChildrenEnumerable = scores.Select((s, index) => CreateDrawableScore(s, index + 1))
            }, newFlow =>
            {
                setState(LeaderboardState.Success);

                scrollContainer.Add(scoreFlowContainer = newFlow);

                double delay = 0;

                foreach (var s in scoreFlowContainer.Children)
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

        private void setState(LeaderboardState state)
        {
            if (state == this.state)
                return;

            if (state == LeaderboardState.Retrieving)
                loading.Show();
            else
                loading.Hide();

            this.state = state;

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
                        Action = RefetchScores
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
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Whether the leaderboard is displaying scores (at least one).
        /// </summary>
        public bool HasScores()
        {
            return state == LeaderboardState.Success;
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

            foreach (var c in scoreFlowContainer.Children)
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
