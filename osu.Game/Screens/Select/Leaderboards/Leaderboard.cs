// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Framework.Logging;
using osu.Game.Rulesets;
using osu.Framework.Input;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class Leaderboard : Container
    {
        private const double fade_duration = 200;

        private readonly ScrollContainer scrollContainer;
        private readonly Container placeholderContainer;

        private FillFlowContainer<LeaderboardScore> scrollFlow;

        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        public Action<Score> ScoreSelected;

        private readonly LoadingAnimation loading;

        private IEnumerable<Score> scores;

        public IEnumerable<Score> Scores
        {
            get { return scores; }
            set
            {
                scores = value;

                scrollFlow?.FadeOut(fade_duration).Expire();
                scrollFlow = null;

                loading.Hide();

                if (scores == null || !scores.Any())
                    return;

                // ensure placeholder is hidden when displaying scores
                PlaceholderState = PlaceholderState.Successful;

                // schedule because we may not be loaded yet (LoadComponentAsync complains).
                Schedule(() =>
                {
                    LoadComponentAsync(new FillFlowContainer<LeaderboardScore>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0f, 5f),
                        Padding = new MarginPadding { Top = 10, Bottom = 5 },
                        ChildrenEnumerable = scores.Select((s, index) => new LeaderboardScore(s, index + 1) { Action = () => ScoreSelected?.Invoke(s) })
                    }, f =>
                    {
                        scrollContainer.Add(scrollFlow = f);

                        int i = 0;
                        foreach (var s in f.Children)
                        {
                            using (s.BeginDelayedSequence(i++ * 50, true))
                                s.Show();
                        }

                        scrollContainer.ScrollTo(0f, false);
                    });
                });
            }
        }

        private LeaderboardScope scope;
        public LeaderboardScope Scope
        {
            get { return scope; }
            set
            {
                if (value == scope) return;

                scope = value;
                updateScores();
            }
        }

        private PlaceholderState placeholderState;
        protected PlaceholderState PlaceholderState
        {
            get { return placeholderState; }
            set
            {
                if (value == placeholderState) return;

                switch (placeholderState = value)
                {
                    case PlaceholderState.NetworkFailure:
                        replacePlaceholder(new RetrievalFailurePlaceholder
                        {
                            OnRetry = updateScores,
                        });
                        break;

                    case PlaceholderState.NoScores:
                        replacePlaceholder(new MessagePlaceholder(@"No records yet!"));
                        break;

                    case PlaceholderState.NotLoggedIn:
                        replacePlaceholder(new MessagePlaceholder(@"Please login to view online leaderboards!"));
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
                    Alpha = 0,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
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
                if (beatmap == value) return;

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
            getScoresRequest?.Cancel();
            getScoresRequest = null;
            Scores = null;

            if (Scope == LeaderboardScope.Local)
            {
                // TODO: get local scores from wherever here.
                PlaceholderState = PlaceholderState.NoScores;
                return;
            }

            if (api?.IsLoggedIn != true)
            {
                PlaceholderState = PlaceholderState.NotLoggedIn;
                return;
            }

            if (Beatmap?.OnlineBeatmapID == null)
            {
                PlaceholderState = PlaceholderState.NetworkFailure;
                return;
            }

            PlaceholderState = PlaceholderState.Retrieving;
            loading.Show();

            if (Scope != LeaderboardScope.Global && !api.LocalUser.Value.IsSupporter)
            {
                loading.Hide();
                PlaceholderState = PlaceholderState.NotSupporter;
                return;
            }

            getScoresRequest = new GetScoresRequest(Beatmap, osuGame?.Ruleset.Value ?? Beatmap.Ruleset, Scope);
            getScoresRequest.Success += r =>
            {
                Scores = r.Scores;
                PlaceholderState = Scores.Any() ? PlaceholderState.Successful : PlaceholderState.NoScores;
            };
            getScoresRequest.Failure += onUpdateFailed;

            api.Queue(getScoresRequest);
        }

        private void onUpdateFailed(Exception e)
        {
            if (e is OperationCanceledException) return;

            PlaceholderState = PlaceholderState.NetworkFailure;
            Logger.Error(e, @"Couldn't fetch beatmap scores!");
        }

        private void replacePlaceholder(Placeholder placeholder)
        {
            if (placeholder == null)
            {
                placeholderContainer.FadeOutFromOne(fade_duration, Easing.OutQuint);
                placeholderContainer.Clear(true);
                return;
            }

            var existingPlaceholder = placeholderContainer.Children.FirstOrDefault() as Placeholder;

            if (placeholder.Equals(existingPlaceholder))
                return;

            Scores = null;

            placeholderContainer.Clear(true);
            placeholderContainer.Child = placeholder;
            placeholderContainer.FadeInFromZero(fade_duration, Easing.OutQuint);
        }

        protected override void Update()
        {
            base.Update();

            var fadeStart = scrollContainer.Current + scrollContainer.DrawHeight;

            if (!scrollContainer.IsScrolledToEnd())
                fadeStart -= LeaderboardScore.HEIGHT;

            if (scrollFlow == null) return;

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

        private abstract class Placeholder : FillFlowContainer, IEquatable<Placeholder>
        {
            public virtual bool Equals(Placeholder other) => GetType() == other?.GetType();
        }

        private class MessagePlaceholder : Placeholder
        {
            private readonly string message;

            public MessagePlaceholder(string message)
            {
                Direction = FillDirection.Horizontal;
                AutoSizeAxes = Axes.Both;
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Icon = FontAwesome.fa_exclamation_circle,
                        Size = new Vector2(26),
                        Margin = new MarginPadding { Right = 10 },
                    },
                    new OsuSpriteText
                    {
                        Text = this.message = message,
                        TextSize = 22,
                    },
                };
            }

            public override bool Equals(Placeholder other) => (other as MessagePlaceholder)?.message == message;
        }

        private class RetrievalFailurePlaceholder : Placeholder
        {
            public Action OnRetry;

            public RetrievalFailurePlaceholder()
            {
                Direction = FillDirection.Horizontal;
                AutoSizeAxes = Axes.Both;
                Children = new Drawable[]
                {
                    new RetryButton
                    {
                        Action = () => OnRetry?.Invoke(),
                        Margin = new MarginPadding { Right = 10 },
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopLeft,
                        Text = @"Couldn't retrieve scores!",
                        TextSize = 22,
                    },
                };
            }

            private class RetryButton : OsuHoverContainer
            {
                private readonly SpriteIcon icon;

                public Action Action;

                public RetryButton()
                {
                    Height = 26;
                    Width = 26;
                    Child = new OsuClickableContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Action = () => Action?.Invoke(),
                        Child = icon = new SpriteIcon
                        {
                            Icon = FontAwesome.fa_refresh,
                            Size = new Vector2(26),
                            Shadow = true,
                        },
                    };
                }

                protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
                {
                    icon.ScaleTo(0.8f, 4000, Easing.OutQuint);
                    return base.OnMouseDown(state, args);
                }

                protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
                {
                    icon.ScaleTo(1, 1000, Easing.OutElastic);
                    return base.OnMouseUp(state, args);
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
        NoScores,
        NotLoggedIn,
        NotSupporter,
    }
}
