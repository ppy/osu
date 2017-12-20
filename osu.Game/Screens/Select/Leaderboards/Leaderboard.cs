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

        public Action<Score> ScoreSelected;

        private readonly LoadingAnimation loading;

        private IEnumerable<Score> scores;

        public IEnumerable<Score> Scores
        {
            get { return scores; }
            set
            {
                scores = value;

                placeholderContainer.FadeOut(fade_duration);

                scrollFlow?.FadeOut(fade_duration).Expire();
                scrollFlow = null;

                loading.Hide();

                if (scores == null)
                    return;

                if (!scores.Any())
                {
                    replacePlaceholder(new MessagePlaceholder(@"No records yet!"));
                    return;
                }

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
                UpdateScores();
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
                pendingBeatmapSwitch = Schedule(UpdateScores);
            }
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(APIAccess api, OsuGame osuGame)
        {
            this.api = api;
            this.osuGame = osuGame;

            if (osuGame != null)
                osuGame.Ruleset.ValueChanged += handleRulesetChange;

            if (api != null)
                api.OnStateChange += handleApiStateChange;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (osuGame != null)
                osuGame.Ruleset.ValueChanged -= handleRulesetChange;
        }

        private GetScoresRequest getScoresRequest;

        private void handleRulesetChange(RulesetInfo ruleset) => UpdateScores();

        private void handleApiStateChange(APIState oldState, APIState newState)
        {
            if (Scope == LeaderboardScope.Local)
                // No need to respond to API state change while current scope is local
                return;

            if (newState == APIState.Online)
                UpdateScores();
        }

        protected virtual void UpdateScores()
        {
            if (!IsLoaded) return;

            getScoresRequest?.Cancel();
            getScoresRequest = null;

            Scores = null;

            if (api == null || Beatmap?.OnlineBeatmapID == null) return;

            loading.Show();

            if (Scope == LeaderboardScope.Local)
            {
                // TODO: get local scores from wherever here.
                Scores = Enumerable.Empty<Score>();
                return;
            }

            if (!api.IsLoggedIn)
            {
                loading.Hide();
                replacePlaceholder(new MessagePlaceholder(@"Please login to view online leaderboards!"));
                return;
            }

            if (Scope != LeaderboardScope.Global && !api.LocalUser.Value.IsSupporter)
            {
                loading.Hide();
                replacePlaceholder(new MessagePlaceholder(@"Please invest in a supporter tag to view this leaderboard!"));
                return;
            }

            getScoresRequest = new GetScoresRequest(Beatmap, osuGame?.Ruleset.Value ?? Beatmap.Ruleset, Scope);
            getScoresRequest.Success += r =>
            {
                Scores = r.Scores;
            };
            getScoresRequest.Failure += OnUpdateFailed;

            api.Queue(getScoresRequest);
        }

        protected void OnUpdateFailed(Exception e)
        {
            if (e is OperationCanceledException) return;

            Scores = null;
            replacePlaceholder(new RetrievalFailurePlaceholder
            {
                OnRetry = UpdateScores,
            });
            Logger.Error(e, @"Couldn't fetch beatmap scores!");
        }

        private void replacePlaceholder(Drawable placeholder)
        {
            placeholderContainer.FadeOutFromOne(fade_duration, Easing.OutQuint);
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

        private class MessagePlaceholder : FillFlowContainer
        {
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
                        Text = message,
                        TextSize = 22,
                    },
                };
            }
        }

        private class RetrievalFailurePlaceholder : FillFlowContainer
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
                icon.ScaleTo(0.8f, 400, Easing.OutQuint);
                return base.OnMouseDown(state, args);
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                icon.ScaleTo(1, 400, Easing.OutElastic);
                return base.OnMouseUp(state, args);
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
}
