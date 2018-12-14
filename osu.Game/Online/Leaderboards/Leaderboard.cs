// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Online.Leaderboards
{
    public abstract class Leaderboard<TScope, TScoreModel> : Container
    {
        private const double fade_duration = 300;

        private readonly ScrollContainer scrollContainer;
        private readonly Container placeholderContainer;

        private FillFlowContainer<LeaderboardScore<TScoreModel>> scrollFlow;

        private readonly LoadingAnimation loading;

        private ScheduledDelegate showScoresDelegate;

        private bool scoresLoadedOnce;

        private IEnumerable<TScoreModel> scores;

        public IEnumerable<TScoreModel> Scores
        {
            get { return scores; }
            set
            {
                scores = value;

                scoresLoadedOnce = true;

                scrollFlow?.FadeOut(fade_duration, Easing.OutQuint).Expire();
                scrollFlow = null;

                loading.Hide();

                if (scores == null || !scores.Any())
                    return;

                // ensure placeholder is hidden when displaying scores
                PlaceholderState = PlaceholderState.Successful;

                var flow = scrollFlow = new FillFlowContainer<LeaderboardScore<TScoreModel>>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0f, 5f),
                    Padding = new MarginPadding { Top = 10, Bottom = 5 },
                    ChildrenEnumerable = scores.Select((s, index) => CreateScoreVisualiser(s, index + 1))
                };

                // schedule because we may not be loaded yet (LoadComponentAsync complains).
                showScoresDelegate?.Cancel();
                if (!IsLoaded)
                    showScoresDelegate = Schedule(showScores);
                else
                    showScores();

                void showScores() => LoadComponentAsync(flow, _ =>
                {
                    scrollContainer.Add(flow);

                    int i = 0;
                    foreach (var s in flow.Children)
                    {
                        using (s.BeginDelayedSequence(i++ * 50, true))
                            s.Show();
                    }

                    scrollContainer.ScrollTo(0f, false);
                });
            }
        }

        private TScope scope;

        public TScope Scope
        {
            get { return scope; }
            set
            {
                if (value.Equals(scope))
                    return;

                scope = value;
                UpdateScores();
            }
        }

        private PlaceholderState placeholderState;

        /// <summary>
        /// Update the placeholder visibility.
        /// Setting this to anything other than PlaceholderState.Successful will cancel all existing retrieval requests and hide scores.
        /// </summary>
        protected PlaceholderState PlaceholderState
        {
            get { return placeholderState; }
            set
            {
                if (value != PlaceholderState.Successful)
                {
                    getScoresRequest?.Cancel();
                    getScoresRequest = null;
                    Scores = null;
                }

                if (value == placeholderState)
                    return;

                switch (placeholderState = value)
                {
                    case PlaceholderState.NetworkFailure:
                        replacePlaceholder(new RetrievalFailurePlaceholder
                        {
                            OnRetry = UpdateScores,
                        });
                        break;
                    case PlaceholderState.Unavailable:
                        replacePlaceholder(new MessagePlaceholder(@"Leaderboards are not available for this beatmap!"));
                        break;
                    case PlaceholderState.NoScores:
                        replacePlaceholder(new MessagePlaceholder(@"No records yet!"));
                        break;
                    case PlaceholderState.NotLoggedIn:
                        replacePlaceholder(new MessagePlaceholder(@"Please sign in to view online leaderboards!"));
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

        protected Leaderboard()
        {
            Children = new Drawable[]
            {
                scrollContainer = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                },
                loading = new LoadingAnimation(),
                placeholderContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both
                },
            };
        }

        private APIAccess api;

        private ScheduledDelegate pendingUpdateScores;

        [BackgroundDependencyLoader(true)]
        private void load(APIAccess api)
        {
            this.api = api;

            if (api != null)
                api.OnStateChange += handleApiStateChange;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (api != null)
                api.OnStateChange -= handleApiStateChange;
        }

        public void RefreshScores() => UpdateScores();

        private APIRequest getScoresRequest;

        private void handleApiStateChange(APIState oldState, APIState newState)
        {
            if (newState == APIState.Online)
                UpdateScores();
        }

        protected void UpdateScores()
        {
            // don't display any scores or placeholder until the first Scores_Set has been called.
            // this avoids scope changes flickering a "no scores" placeholder before initialisation of song select is finished.
            if (!scoresLoadedOnce) return;

            getScoresRequest?.Cancel();
            getScoresRequest = null;

            pendingUpdateScores?.Cancel();
            pendingUpdateScores = Schedule(() =>
            {
                if (api?.IsLoggedIn != true)
                {
                    PlaceholderState = PlaceholderState.NotLoggedIn;
                    return;
                }

                PlaceholderState = PlaceholderState.Retrieving;
                loading.Show();

                getScoresRequest = FetchScores(scores => Schedule(() =>
                {
                    Scores = scores;
                    PlaceholderState = Scores.Any() ? PlaceholderState.Successful : PlaceholderState.NoScores;
                }));

                if (getScoresRequest == null)
                    return;

                getScoresRequest.Failure += e => Schedule(() =>
                {
                    if (e is OperationCanceledException)
                        return;

                    PlaceholderState = PlaceholderState.NetworkFailure;
                });

                api.Queue(getScoresRequest);
            });
        }

        protected abstract APIRequest FetchScores(Action<IEnumerable<TScoreModel>> scoresCallback);

        private Placeholder currentPlaceholder;

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

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            var fadeStart = scrollContainer.Current + scrollContainer.DrawHeight;

            if (!scrollContainer.IsScrolledToEnd())
                fadeStart -= LeaderboardScore.HEIGHT;

            if (scrollFlow == null)
                return;

            foreach (var c in scrollFlow.Children)
            {
                var topY = c.ToSpaceOfOtherDrawable(Vector2.Zero, scrollFlow).Y;
                var bottomY = topY + LeaderboardScore.HEIGHT;

                if (bottomY < fadeStart)
                    c.Colour = Color4.White;
                else if (topY > fadeStart + LeaderboardScore.HEIGHT)
                    c.Colour = Color4.Transparent;
                else
                {
                    c.Colour = ColourInfo.GradientVertical(
                        Color4.White.Opacity(Math.Min(1 - (topY - fadeStart) / LeaderboardScore.HEIGHT, 1)),
                        Color4.White.Opacity(Math.Min(1 - (bottomY - fadeStart) / LeaderboardScore.HEIGHT, 1)));
                }
            }
        }

        protected abstract LeaderboardScore<TScoreModel> CreateScoreVisualiser(TScoreModel model, int index);
    }
}
