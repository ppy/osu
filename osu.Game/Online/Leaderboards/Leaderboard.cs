// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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

namespace osu.Game.Online.Leaderboards
{
    /// <summary>
    /// A leaderboard which displays a scrolling list of top scores, along with a single "user best"
    /// for the local user.
    /// </summary>
    /// <typeparam name="TScope">The scope of the leaderboard (ie. global or local).</typeparam>
    /// <typeparam name="TScoreInfo">The score model class.</typeparam>
    public abstract class Leaderboard<TScope, TScoreInfo> : CompositeDrawable
    {
        /// <summary>
        /// Whether the current scope should refetch in response to changes in API connectivity state.
        /// </summary>
        protected abstract bool IsOnlineScope { get; }

        private const double fade_duration = 300;

        private readonly OsuScrollContainer scrollContainer;
        private readonly Container placeholderContainer;
        private readonly UserTopScoreContainer<TScoreInfo> topScoreContainer;

        private FillFlowContainer<LeaderboardScore> scrollFlow;

        private readonly LoadingSpinner loading;

        private CancellationTokenSource currentFetchCancellationSource;

        private APIRequest fetchScoresRequest;

        [Resolved(CanBeNull = true)]
        private IAPIProvider api { get; set; }

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        private ICollection<TScoreInfo> scores;

        public ICollection<TScoreInfo> Scores
        {
            get => scores;
            protected set
            {
                scores = value;
                updateScoresDrawables();
            }
        }

        public TScoreInfo TopScore
        {
            get => topScoreContainer.Score.Value;
            set
            {
                topScoreContainer.Score.Value = value;

                if (value == null)
                    topScoreContainer.Hide();
                else
                    topScoreContainer.Show();
            }
        }

        private TScope scope;

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
                                new Container
                                {
                                    AutoSizeAxes = Axes.Y,
                                    RelativeSizeAxes = Axes.X,
                                    Child = topScoreContainer = new UserTopScoreContainer<TScoreInfo>(CreateDrawableTopScore)
                                },
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

        public void RefetchScores() => Scheduler.AddOnce(refetchScores);

        protected virtual void Reset()
        {
            cancelPendingWork();
            Scores = null;
        }

        /// <summary>
        /// Performs a fetch/refresh of scores to be displayed.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>An <see cref="APIRequest"/> responsible for the fetch operation. This will be queued and performed automatically.</returns>
        [CanBeNull]
        protected abstract APIRequest FetchScores(CancellationToken cancellationToken);

        protected abstract LeaderboardScore CreateDrawableScore(TScoreInfo model, int index);

        protected abstract LeaderboardScore CreateDrawableTopScore(TScoreInfo model);

        private void refetchScores()
        {
            Reset();

            PlaceholderState = PlaceholderState.Retrieving;
            loading.Show();

            currentFetchCancellationSource = new CancellationTokenSource();

            fetchScoresRequest = FetchScores(currentFetchCancellationSource.Token);

            if (fetchScoresRequest == null)
                return;

            fetchScoresRequest.Failure += e => Schedule(() =>
            {
                if (e is OperationCanceledException || currentFetchCancellationSource.IsCancellationRequested)
                    return;

                PlaceholderState = PlaceholderState.NetworkFailure;
            });

            api?.Queue(fetchScoresRequest);
        }

        private void cancelPendingWork()
        {
            currentFetchCancellationSource?.Cancel();
            currentFetchCancellationSource = null;

            fetchScoresRequest?.Cancel();
            fetchScoresRequest = null;
        }

        #region Placeholder handling

        private Placeholder currentPlaceholder;

        private PlaceholderState placeholderState;

        /// <summary>
        /// Update the placeholder visibility.
        /// Setting this to anything other than PlaceholderState.Successful will cancel all existing retrieval requests and hide scores.
        /// </summary>
        protected PlaceholderState PlaceholderState
        {
            get => placeholderState;
            set
            {
                if (value == placeholderState)
                    return;

                switch (placeholderState = value)
                {
                    case PlaceholderState.NetworkFailure:
                        replacePlaceholder(new ClickablePlaceholder(@"Couldn't fetch scores!", FontAwesome.Solid.Sync)
                        {
                            Action = RefetchScores
                        });
                        break;

                    case PlaceholderState.NoneSelected:
                        replacePlaceholder(new MessagePlaceholder(@"Please select a beatmap!"));
                        break;

                    case PlaceholderState.Unavailable:
                        replacePlaceholder(new MessagePlaceholder(@"Leaderboards are not available for this beatmap!"));
                        break;

                    case PlaceholderState.NoScores:
                        replacePlaceholder(new MessagePlaceholder(@"No records yet!"));
                        break;

                    case PlaceholderState.NotLoggedIn:
                        replacePlaceholder(new LoginPlaceholder(@"Please sign in to view online leaderboards!"));
                        break;

                    case PlaceholderState.NotSupporter:
                        replacePlaceholder(new MessagePlaceholder(@"Please invest in an osu!supporter tag to view this leaderboard!"));
                        break;

                    default:
                        replacePlaceholder(null);
                        break;
                }
            }
        }

        private void updateScoresDrawables() => Scheduler.Add(() =>
        {
            scrollFlow?.FadeOut(fade_duration, Easing.OutQuint).Expire();
            scrollFlow = null;

            if (scores?.Any() != true)
            {
                loading.Hide();
                PlaceholderState = PlaceholderState.NoScores;
                return;
            }

            Debug.Assert(!currentFetchCancellationSource.IsCancellationRequested);

            // ensure placeholder is hidden when displaying scores
            PlaceholderState = PlaceholderState.Successful;

            var scoreFlow = new FillFlowContainer<LeaderboardScore>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0f, 5f),
                Padding = new MarginPadding { Top = 10, Bottom = 5 },
                ChildrenEnumerable = scores.Select((s, index) => CreateDrawableScore(s, index + 1))
            };

            // schedule because we may not be loaded yet (LoadComponentAsync complains).
            LoadComponentAsync(scoreFlow, _ =>
            {
                scrollContainer.Add(scrollFlow = scoreFlow);

                int i = 0;

                foreach (var s in scrollFlow.Children)
                {
                    using (s.BeginDelayedSequence(i++ * 50))
                        s.Show();
                }

                scrollContainer.ScrollToStart(false);
                loading.Hide();
            }, currentFetchCancellationSource.Token);
        }, false);

        private void replacePlaceholder(Placeholder placeholder)
        {
            if (placeholder != null && placeholder.Equals(currentPlaceholder))
                return;

            currentPlaceholder?.FadeOut(150, Easing.OutQuint).Expire();

            if (placeholder == null)
            {
                currentPlaceholder = null;
                return;
            }

            placeholderContainer.Child = placeholder;

            placeholder.ScaleTo(0.8f).Then().ScaleTo(1, fade_duration * 3, Easing.OutQuint);
            placeholder.FadeInFromZero(fade_duration, Easing.OutQuint);

            currentPlaceholder = placeholder;
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

            if (scrollFlow == null)
                return;

            foreach (var c in scrollFlow.Children)
            {
                float topY = c.ToSpaceOfOtherDrawable(Vector2.Zero, scrollFlow).Y;
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
