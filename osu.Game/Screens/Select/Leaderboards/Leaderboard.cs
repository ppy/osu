// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Scoring;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Logging;
using osu.Game.Rulesets;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class Leaderboard : Container
    {
        private const double fade_duration = 300;

        private readonly ScrollContainer scrollContainer;
        private readonly Container placeholderContainer;

        private FillFlowContainer<LeaderboardScore> scrollFlow;

        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        public Action<Score> ScoreSelected;

        private readonly LoadingAnimation loading;

        private ScheduledDelegate showScoresDelegate;

        private IEnumerable<Score> scores;
        public IEnumerable<Score> Scores
        {
            get { return scores; }
            set
            {
                scores = value;

                scrollFlow?.FadeOut(fade_duration, Easing.OutQuint).Expire();
                scrollFlow = null;

                loading.Hide();

                if (scores == null || !scores.Any())
                    return;

                // ensure placeholder is hidden when displaying scores
                PlaceholderState = PlaceholderState.Successful;

                var flow = scrollFlow = new FillFlowContainer<LeaderboardScore>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0f, 5f),
                    Padding = new MarginPadding { Top = 10, Bottom = 5 },
                    ChildrenEnumerable = scores.Select((s, index) => new LeaderboardScore(s, index + 1) { Action = () => ScoreSelected?.Invoke(s) })
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

        private LeaderboardScope scope;

        public LeaderboardScope Scope
        {
            get { return scope; }
            set
            {
                if (value == scope)
                    return;

                scope = value;
                updateScores();
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
                            OnRetry = updateScores,
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
                        replacePlaceholder(new MessagePlaceholder(@"Please invest in a supporter tag to view this leaderboard!"));
                        break;
                    default:
                        replacePlaceholder(null);
                        break;
                }
            }
        }

        public Leaderboard()
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
        private BeatmapInfo beatmap;
        private OsuGame osuGame;

        private ScheduledDelegate pendingBeatmapSwitch;

        public BeatmapInfo Beatmap
        {
            get { return beatmap; }
            set
            {
                if (beatmap == value)
                    return;

                beatmap = value;
                Scores = null;

                pendingBeatmapSwitch?.Cancel();
                pendingBeatmapSwitch = Schedule(updateScores);
            }
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(APIAccess api, OsuGame osuGame)
        {
            this.api = api;
            this.osuGame = osuGame;

            if (osuGame != null)
                ruleset.BindTo(osuGame.Ruleset);

            ruleset.ValueChanged += r => updateScores();

            if (api != null)
                api.OnStateChange += handleApiStateChange;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (api != null)
                api.OnStateChange -= handleApiStateChange;
        }

        private GetScoresRequest getScoresRequest;

        private void handleApiStateChange(APIState oldState, APIState newState)
        {
            if (Scope == LeaderboardScope.Local)
                // No need to respond to API state change while current scope is local
                return;

            if (newState == APIState.Online)
                updateScores();
        }

        private void updateScores()
        {
            if (Scope == LeaderboardScope.Local)
            {
                // TODO: get local scores from wherever here.
                PlaceholderState = PlaceholderState.NoScores;
                return;
            }

            if (Beatmap?.OnlineBeatmapID == null)
            {
                PlaceholderState = PlaceholderState.Unavailable;
                return;
            }

            if (api?.IsLoggedIn != true)
            {
                PlaceholderState = PlaceholderState.NotLoggedIn;
                return;
            }

            if (Scope != LeaderboardScope.Global && !api.LocalUser.Value.IsSupporter)
            {
                PlaceholderState = PlaceholderState.NotSupporter;
                return;
            }

            PlaceholderState = PlaceholderState.Retrieving;
            loading.Show();

            getScoresRequest = new GetScoresRequest(Beatmap, osuGame?.Ruleset.Value ?? Beatmap.Ruleset, Scope);
            getScoresRequest.Success += r => Schedule(() =>
            {
                Scores = r.Scores;
                PlaceholderState = Scores.Any() ? PlaceholderState.Successful : PlaceholderState.NoScores;
            });

            getScoresRequest.Failure += e => Schedule(() =>
            {
                if (e is OperationCanceledException)
                    return;

                PlaceholderState = PlaceholderState.NetworkFailure;
                Logger.Error(e, @"Couldn't fetch beatmap scores!");
            });

            api.Queue(getScoresRequest);
        }

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

        protected override void Update()
        {
            base.Update();

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
    }

    public enum LeaderboardScope
    {
        Local,
        Country,
        Global,
        Friend,
    }

    public enum PlaceholderState
    {
        Successful,
        Retrieving,
        NetworkFailure,
        Unavailable,
        NoScores,
        NotLoggedIn,
        NotSupporter,
    }
}
