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
using System.Net;
using osu.Game.Rulesets;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class Leaderboard : Container
    {
        private const double fade_duration = 200;

        private readonly ScrollContainer scrollContainer;
        private FillFlowContainer<LeaderboardScore> scrollFlow;
        private Container noResultsPlaceholder;
        private Container retryPlaceholder;

        public Action<Score> ScoreSelected;

        private readonly LoadingAnimation loading;

        private IEnumerable<Score> scores;

        public IEnumerable<Score> Scores
        {
            get { return scores; }
            set
            {
                scores = value;
                getScoresRequest?.Cancel();
                getScoresRequest = null;

                noResultsPlaceholder.FadeOut(fade_duration);
                scrollFlow?.FadeOut(fade_duration).Expire();
                scrollContainer.Clear(true); // scores stick around in scrollFlow in VisualTests without this for some reason
                scrollFlow = null;

                loading.Hide();

                if (scores == null)
                    return;

                if (scores.Count() == 0)
                {
                    noResultsPlaceholder.FadeIn(fade_duration);
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
                updateScores();
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
                noResultsPlaceholder = new Container
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Horizontal,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
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
                                    Text = @"No records yet!",
                                    TextSize = 22,
                                },
                            }
                        },
                    },
                },
                retryPlaceholder = new Container
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Horizontal,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new RetryButton
                                {
                                    Action = updateScores,
                                    Margin = new MarginPadding { Right = 10 },
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.TopLeft,
                                    Text = @"An error occurred!",
                                    TextSize = 22,
                                },
                            }
                        },
                    },
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
                osuGame.Ruleset.ValueChanged += handleRulesetChange;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (osuGame != null)
                osuGame.Ruleset.ValueChanged -= handleRulesetChange;
        }

        private GetScoresRequest getScoresRequest;

        private void handleRulesetChange(RulesetInfo ruleset) => updateScores();

        private void updateScores()
        {
            if (!IsLoaded) return;

            retryPlaceholder.FadeOut(fade_duration);

            Scores = null;

            if (api == null || Beatmap?.OnlineBeatmapID == null) return;

            if (Scope == LeaderboardScope.Local)
            {
                // TODO: get local scores from wherever here.
                Scores = Enumerable.Empty<Score>();
                return;
            }

            loading.Show();

            getScoresRequest = new GetScoresRequest(Beatmap, osuGame?.Ruleset.Value, Scope);
            getScoresRequest.Success += r =>
            {
                Scores = r.Scores;
            };
            getScoresRequest.Failure += e =>
            {
                // TODO: check why failure is repeatedly invoked even on successful requests
                if (e is WebException)
                {
                    Scores = null;
                    retryPlaceholder.FadeIn(fade_duration);
                    Logger.Error(e, @"Couldn't fetch beatmap scores!");
                }
            };

            api.Queue(getScoresRequest);
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

        private class RetryButton : ClickableContainer
        {
            private SpriteIcon icon;

            public RetryButton()
            {
                Height = 26;
                Width = 26;
                Children = new Drawable[]
                {
                    icon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.fa_refresh,
                        Size = new Vector2(26),
                    }
                };
            }

            protected override bool OnHover(Framework.Input.InputState state)
            {
                icon.ScaleTo(1.4f, 400, Easing.OutQuint);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(Framework.Input.InputState state)
            {
                icon.ScaleTo(1f, 400, Easing.OutQuint);
                base.OnHoverLost(state);
            }

            protected override bool OnMouseDown(Framework.Input.InputState state, Framework.Input.MouseDownEventArgs args)
            {
                icon.ScaleTo(0.8f, 200, Easing.InElastic);
                return base.OnMouseDown(state, args);
            }

            protected override bool OnMouseUp(Framework.Input.InputState state, Framework.Input.MouseUpEventArgs args)
            {
                icon.ScaleTo(1.2f, 400, Easing.OutElastic).Then().ScaleTo(1f, 400, Easing.OutElastic);
                return base.OnMouseUp(state, args);
            }
        }
    }

    public enum LeaderboardScope
    {
        Local,
        Country,
        Global,
        Friends,
    }
}
